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
    }
}