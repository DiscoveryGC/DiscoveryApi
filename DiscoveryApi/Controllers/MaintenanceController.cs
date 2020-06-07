using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiscoveryApi.Models;
using DiscoveryApi.DAL;
using DiscoveryApi.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscoveryApi.Controllers
{
    [Route("[controller]/[action]")]
    public class MaintenanceController : Controller
    {
        private readonly ILogger<MaintenanceController> logger;
        private readonly apiContext context;
        private readonly IOptions<ConfigModel> config;
        public MaintenanceController(apiContext _context, IOptions<ConfigModel> _config, ILogger<MaintenanceController> _logger)
        {
            context = _context;
            config = _config;
            logger = _logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private bool isValidKey(string key)
        {
            if (context.ApiKeys.Any(c => c.Key == key && c.Admin == true))
                return true;
            else
                return false;
        }

        [HttpGet("{key}")]
        public string CollatePlayerCounts(string key)
        {
            if (!isValidKey(key))
            {
                return "NO";
            }

            //This is dumb as hell but we have to do it 
            //Get all player data

            var data = context.ServerPlayercounts.OrderBy(c => c.Date).ToList();
            if (data.Count > 0)
            {
                DateTime FirstTime = data.First().Date;
                //Make a proper date
                DateTime LastTime = new DateTime(FirstTime.Year, FirstTime.Month, FirstTime.Day, FirstTime.Hour, 0, 0);
                List<UpdatePlayerCountStruct> NewData = new List<UpdatePlayerCountStruct>();
                foreach (var item in data)
                {
                    if (item.Date >= LastTime.AddHours(1))
                    {
                        //There is likely to be huge gaps due to downtimes, so we have to consider it.
                        LastTime = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, item.Date.Hour, 0, 0);
                        NewData.Add(new UpdatePlayerCountStruct { Date = LastTime, Count = item.PlayerCount });
                    }
                    else
                    {
                        //Check the current value or create it if it's the first item
                        if (NewData.Any(c => c.Date == LastTime))
                        {
                            var it = NewData.SingleOrDefault(c => c.Date == LastTime);
                            if (item.PlayerCount > it.Count)
                            {
                                it.Count = item.PlayerCount;
                            }
                        }
                        else
                        {
                            //Create the initial record
                            NewData.Add(new UpdatePlayerCountStruct { Date = LastTime, Count = item.PlayerCount });
                        }
                    }
                }

                //Destroy all current data
                context.ServerPlayercounts.RemoveRange(context.ServerPlayercounts.ToList());
                context.SaveChanges();

                List<ServerPlayercounts> NewEntities = new List<ServerPlayercounts>();
                //Insert our new data
                foreach (var item in NewData)
                {
                    var it = new ServerPlayercounts();
                    it.Date = item.Date;
                    it.PlayerCount = item.Count;
                    NewEntities.Add(it);
                }
                context.AddRange(NewEntities);
                context.SaveChanges();
                return "OK";
            }

            return "Nothing.";
        }

        [HttpGet("{key}")]
        public string FixConnData(string key)
        {
            if (!isValidKey(key))
            {
                return "NO";
            }

            using (var ctx = new apiContext())
            {
                //another layer as otherwise it fills the server's memory :O
                int sessions_count = ctx.ServerSessions.Count();
                int session_selection_count = 5000;

                for (int session_current_count = 0; session_current_count < sessions_count; session_current_count = session_current_count + session_selection_count)
                {
                    var session_target = session_current_count + session_selection_count;
                    var sessions = ctx.ServerSessions
                        .Include(c => c.ServerSessionsDataConn)
                        .Where(c => c.SessionId >= session_current_count && c.SessionId < session_target)
                        .Where(c => c.SessionEnd.HasValue && c.ServerSessionsDataConn.Count > 0)
                        .OrderByDescending(c => c.SessionId)
                        .ToList();

                    foreach (var session in sessions)
                    {
                        var list = session.ServerSessionsDataConn.OrderBy(c => c.Stamp).ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] != list.First())
                            {
                                TimeSpan span = list[i].Stamp - list[i - 1].Stamp;
                                list[i].Duration = (int)span.TotalSeconds;
                            }
                            else
                            {
                                //this is the first entry, use the session start
                                TimeSpan span = list[i].Stamp - session.SessionStart;
                                list[i].Duration = (int)span.TotalSeconds;
                            }
                        }                      
                    }

                    //Server does not survive big transactions, so we will have to force smaller transactions
                    ctx.SaveChanges();
                    logger.LogInformation("FixConnData: " + session_target.ToString() + "/" + sessions_count.ToString());
                }

                ctx.SaveChanges();
            }

            
            return "OK";
        }

        [HttpGet("{key}")]
        public string CollateConnData(string key)
        {
            //What this does is remove multiple entries for the same system. Players change systems quite often and we do not need pinpoint accuracy of where they were at a given second.
            if (!isValidKey(key))
            {
                return "NO";
            }

            using (var ctx = new apiContext())
            {
                //another layer as otherwise it fills the server's memory :O
                int sessions_count = ctx.ServerSessions.Count();
                int session_selection_count = 5000;

                for (int session_current_count = 0; session_current_count < sessions_count; session_current_count = session_current_count + session_selection_count)
                {
                    var session_target = session_current_count + session_selection_count;
                    var sessions = ctx.ServerSessions
                        .Include(c => c.ServerSessionsDataConn)
                        .Where(c => c.SessionId >= session_current_count && c.SessionId < session_target)
                        .Where(c => c.SessionEnd.HasValue && c.ServerSessionsDataConn.Count > 0)
                        .OrderByDescending(c => c.SessionId)
                        .ToList();

                    foreach (var session in sessions)
                    {
                        var list = session.ServerSessionsDataConn.OrderBy(c => c.Stamp).ToList();
                        var ListOfSystems = new List<string>();
                        foreach (var item in list)
                        {
                            if (!ListOfSystems.Contains(item.Location.ToUpper()))
                                ListOfSystems.Add(item.Location.ToUpper());
                        }

                        foreach (var item in ListOfSystems)
                        {
                            //Use the oldest entry to keep a minimum of info
                            var Entries = list.Where(c => c.Location.ToUpper() == item).OrderByDescending(c => c.Stamp).ToList();
                            if (Entries.Count > 1)
                            {
                                var First = Entries.First();
                                foreach (var entry in Entries.Where(c => c.Id != First.Id).ToList())
                                {
                                    First.Duration += entry.Duration;
                                    ctx.ServerSessionsDataConn.Remove(entry);
                                }
                            }
                            //Don't care if there is not more than one entry
                        }
                    }

                    //Server does not survive big transactions, so we will have to force smaller transactions
                    ctx.SaveChanges();
                    logger.LogInformation("CollateConnData: " + session_target.ToString() + "/" + sessions_count.ToString());
                }

                ctx.SaveChanges();
            }
            
            return "OK";
        }

        /// <summary>
        /// Generate a static history of faction activity. This is meant for everything prior to the ongoing month. 
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="length">Expressed in months</param>
        /// <returns></returns>
        [HttpGet("{key}/{length}")]
        public string GenerateFactionHistory(string key, int length = 2)
        {
            if (!isValidKey(key))
            {
                return "NO";
            }

            CacheManager cm = CacheManager.Instance;
            //Get all existing server factions
            var Factions = context.ServerFactions.ToList();
            var end_month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
            var start_month = end_month.AddMonths(-length);
            

            //For each faction...
            foreach (var faction in Factions)
            {
                //Potentially existing activity records
                var FactionActivity = context.ServerFactionsActivity.Where(c => c.FactionId == faction.Id).ToList();

                //Start iterating per month
                var curr_start = start_month;
 
                while (curr_start < end_month)
                {
                    ulong total_in_seconds = 0;
                    ulong total_wasted_seconds = 0;

                    var curr_end = new DateTime(curr_start.Year, curr_start.Month, DateTime.DaysInMonth(curr_start.Year, curr_start.Month), 23, 59, 59, 999);

                    //Get all sessions matching the faction tag within that timeframe
                    //Also I don't think it would ever happen but we're going to ensure we only take sessions that have ended
                    IQueryable<ServerSessions> factionSessions = context.ServerSessions;
                    if (faction.FactionTag == "L\\-") {
                        factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE 'L\\\\\\\\-%'");
                    } else if (faction.FactionTag == "|\\/|-") {
                        factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '|\\\\\\\\/|-%'");
                    } else if (faction.FactionTag == "\\*/~") {
                        factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '\\\\\\\\*/~%'");
                    } else if (faction.FactionTag == "[TBH]" || faction.FactionTag == "|Aoi" || faction.FactionTag == "Reaver") {
                        factionSessions = factionSessions.Where(c => c.PlayerName.Contains(faction.FactionTag));
                    } else {
                        factionSessions = factionSessions.Where(c => c.PlayerName.StartsWith(faction.FactionTag));
                    }
                    var sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= curr_start && c.SessionStart <= curr_end && c.SessionEnd.HasValue).ToList();
                    //We also have to go through each system visited in order to revoke wasted activity
                    foreach (var item in sessions)
                    {
                        foreach (var system in item.ServerSessionsDataConn)
                        {
                            if (cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                            {
                                //wasted activity
                                total_wasted_seconds += (ulong)system.Duration;
                            }
                            else
                            {
                                //not wasted
                                total_in_seconds += (ulong)system.Duration;
                            }
                        }
                    }

                    if (FactionActivity.Any(c => c.Stamp == curr_start))
                    {
                        var Data = FactionActivity.SingleOrDefault(c => c.Stamp == curr_start);
                        Data.Duration = total_in_seconds;
                        if (total_wasted_seconds > 0)
                            Data.Duration2 = total_wasted_seconds;
                    }
                    else
                    {
                        var Data = new ServerFactionsActivity();
                        Data.Duration = total_in_seconds;
                        Data.FactionId = faction.Id;
                        Data.Stamp = curr_start;
                        if (total_wasted_seconds > 0)
                            Data.Duration2 = total_wasted_seconds;

                        context.ServerFactionsActivity.Add(Data);
                    }

                    curr_start = curr_start.AddMonths(1);
                }

                //Save for each faction at a time, despite it will likely slow down the process to do that
                context.SaveChanges();
            }

            return "OK";
        }
    }
}