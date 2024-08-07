﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using DiscoveryApi.DAL;
using Newtonsoft.Json.Linq;
using DiscoveryApi.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DiscoveryApi.Utils;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiscoveryApi.Controllers
{
    [Area("api"), Route("[area]/[controller]/[action]")]
    public class UpdateController : Controller
    {

        private readonly ILogger<UpdateController> logger;
        private readonly apiContext context;
        private readonly IOptions<ConfigModel> config;
        public UpdateController(apiContext _context, IOptions<ConfigModel> _config, ILogger<UpdateController> _logger)
        {
            context = _context;
            config = _config;
            logger = _logger;
        }

        // GET: /<controller>/
        [HttpGet("{key}")]
        public JsonResult UpdateData(string key)
        {
            var model = new UpdateModel();
            
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            CacheManager cm = CacheManager.Instance;
            //Get the current data
            string Url = config.Value.ApiSettings.JsonLocation;
            UpdateResponseModel Result = new UpdateResponseModel();
            Result.Players = new List<UpdateResponsePlayerModel>();
            bool Success = false;

            using (var client = new HttpClient())
            {
                //Perhaps we can change this part of the API to async, but it could lead to problems with database access, so for now let's not

                client.BaseAddress = new Uri(Url);
                var request = client.GetAsync("player_status.json");
                request.Wait();

                if (request.Result.IsSuccessStatusCode)
                {
                    Success = true;
                    //This transformation is a bit of a hack, we will discard it when we can change the game server's response structure
                    var Dic = JsonConvert.DeserializeObject<UpdateResponseRawModel>(request.Result.Content.ReadAsStringAsync().Result);
                    DateTime dt = DateTime.ParseExact(Dic.Timestamp, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                    Result.Timestamp = dt;

                    if (Result.Timestamp > CacheManager.Instance.LastUpdate)
                        cm.Retry = 0;

                    foreach (var item in Dic.Players)
                    {
                        var mdl = new UpdateResponsePlayerModel();
                        mdl.Name = item.Key;
                        mdl.Id = item.Value.Id;
                        mdl.Ip = item.Value.Ip;
                        mdl.Lag = item.Value.Lag;
                        mdl.Loss = item.Value.Loss;
                        mdl.Ping = item.Value.Ping;
                        mdl.Ship = item.Value.Ship;
                        mdl.System = item.Value.System;
                        Result.Players.Add(mdl);
                    }
                }

                var cloakRequest = client.GetAsync("player_cloak_status.json");
                cloakRequest.Wait();

                Dictionary<string, string> PlayerObfuscatedSystems = new Dictionary<string, string>();

                if (cloakRequest.Result.IsSuccessStatusCode)
                {
                    var Dic = JsonConvert.DeserializeObject<UpdateResponseCloakRawModel>(cloakRequest.Result.Content.ReadAsStringAsync().Result);
                    foreach (var item in Dic.Players)
                    {
                        PlayerObfuscatedSystems[item.Key] = item.Value.System;
                    }
                }

                foreach (var player in Result.Players)
                {
                    player.ObfuscatedSystem = PlayerObfuscatedSystems.GetValueOrDefault(player.Name, player.System);
                }
            }

            //We are done receiving the data, we can now update our database
            //We are more likely to have a successful request

            if (Success && Result.Timestamp != null && (cm.Retry < cm.MaxRetry))
            {
                if (Result.Timestamp < CacheManager.Instance.LastUpdate)
                {
                    //This can happen often as the cron script and the server updates may not be in perfect sync
                    cm.Retry = cm.Retry + 1;
                    model.Error = Ressources.ApiResource.TimestampRenewTooSoon;
                    logger.LogWarning("Server data has not yet been renewed");
                    return Json(model);
                }

                List<string> ProcessedPlayers = new List<string>();
                var ActivePlayers = context.ServerSessions.Include(c => c.ServerSessionsDataConn).Where(c => !c.SessionEnd.HasValue).ToList();
                //First, we're going to see if we have sessions to end or update
                foreach (var item in ActivePlayers)
                {
                    var last_system = item.ServerSessionsDataConn.OrderBy(c => c.Stamp).LastOrDefault();
                    TimeSpan Diff = Result.Timestamp - last_system.Stamp;

                    //Is the player still online?
                    if (!Result.Players.Any(c => c.Name == item.PlayerName))
                    {
                        //Nope, end the session and compile stats
                        item.SessionEnd = DateTime.UtcNow;
                        //We'll be able to remove these ternary operations later, but for now we have to do it as otherwise it will crash due to open sessions on the current database without any info about the new data tables
                        if (item.ServerSessionsDataConn.Count > 0)
                        {
                            item.PlayerLagAvg = (int)item.ServerSessionsDataConn.Average(c => c.Lag);
                            item.PlayerLossAvg = (int)item.ServerSessionsDataConn.Average(c => c.Loss);
                            item.PlayerPingAvg = (int)item.ServerSessionsDataConn.Average(c => c.Ping);
                            item.PlayerLastShip = item.ServerSessionsDataConn.LastOrDefault().Ship;
                            item.ServerSessionsDataConn.LastOrDefault().Duration += (int)Diff.TotalSeconds;
                        }
                    }
                    else
                    {
                        var PlayerInfo = Result.Players.SingleOrDefault(c => c.Name == item.PlayerName);
                        //We're moving the amount of entries to one per system change instead of one per minute. This will improve performance with minimal differences.
                        //Not checking for null because there is always at least one entry
                        if (last_system.ObfuscatedLocation.ToUpper() == PlayerInfo.ObfuscatedSystem.ToUpper())
                        {
                            //The player hasn't changed systems. Update the current information.
                            last_system.Lag = (last_system.Lag + PlayerInfo.Lag) / 2;
                            last_system.Loss = (last_system.Loss + PlayerInfo.Loss) / 2;
                            last_system.Ping = (last_system.Ping + PlayerInfo.Ping) / 2;
                            last_system.Ship = PlayerInfo.Ship;
                            last_system.Duration += (int)Diff.TotalSeconds;
                            last_system.Stamp = Result.Timestamp;
                        }
                        else
                        {
                            //The player has changed systems, or cloaked/uncloaked
                            //Update the duration of the previous system
                            last_system.Duration += (int)Diff.TotalSeconds;

                            //Create a new system entry
                            var system = new ServerSessionsDataConn();
                            system.SessionId = item.SessionId;
                            system.Stamp = Result.Timestamp;
                            system.Ship = PlayerInfo.Ship;
                            system.Location = PlayerInfo.System;
                            system.ObfuscatedLocation = PlayerInfo.ObfuscatedSystem;
                            system.Ping = PlayerInfo.Ping;
                            system.Lag = PlayerInfo.Lag;
                            system.Loss = PlayerInfo.Loss;
                            system.Duration = 0;
                            item.ServerSessionsDataConn.Add(system);
                        }
                    }

                    ProcessedPlayers.Add(item.PlayerName);
                }

                //We've handled the active players. Now lets handle the new players
                foreach (var item in Result.Players.Where(c => !ProcessedPlayers.Contains(c.Name)))
                {
                    var Session = new ServerSessions();
                    Session.PlayerName = item.Name;
                    Session.PlayerId = item.Id;
                    Session.SessionIp = item.Ip;
                    Session.SessionStart = Result.Timestamp;
                    Session.PlayerLagAvg = 0;
                    Session.PlayerLossAvg = 0;
                    Session.PlayerPingAvg = 0;
                    Session.PlayerLastShip = item.Ship;
                    context.ServerSessions.Add(Session);
                    //We do not compile stats here !!!

                    var system = new ServerSessionsDataConn();
                    system.Session = Session;
                    system.Stamp = Result.Timestamp;
                    system.Duration = 0;
                    system.Location = item.System;
                    system.ObfuscatedLocation = item.ObfuscatedSystem;
                    system.Ship = item.Ship;
                    system.Lag = item.Lag;
                    system.Loss = item.Loss;
                    system.Ping = item.Ping;

                    Session.ServerSessionsDataConn.Add(system);
                }

                //Update the player count, or alternatively create a new entry
                var time = new DateTime(Result.Timestamp.Year, Result.Timestamp.Month, Result.Timestamp.Day, Result.Timestamp.Hour, 0, 0);
                var entry = context.ServerPlayercounts.SingleOrDefault(c => c.Date == time);

                if (entry != null)
                {
                    if (Result.Players.Count > entry.PlayerCount)
                    {
                        entry.PlayerCount = (short)Result.Players.Count;
                    }
                }
                else
                {
                    var newentry = new ServerPlayercounts();
                    newentry.Date = time;
                    newentry.PlayerCount = (short)Result.Players.Count;
                    context.ServerPlayercounts.Add(newentry);
                }

                context.SaveChanges();
                //Update our internal timestamp
                cm.LastUpdate = DateTime.UtcNow;

                model.Error = "OK";
                logger.LogInformation("Successfully retrieved and parsed the server data");

                return Json(model);
            }
            else
            {
                //Has the request failed? If so, end all current sessions.
                var ActivePlayers = context.ServerSessions.Where(c => !c.SessionEnd.HasValue).ToList();
                foreach (var item in ActivePlayers)
                {
                    item.SessionEnd = DateTime.UtcNow;
                }

                context.SaveChanges();
                model.Error = Ressources.ApiResource.UpdateRequestFailed;
                logger.LogWarning("Failed to retrieve the server data");
                return Json(model);
            }
        }

        private string TimeIntToStr(int Time)
        {
            if (Time < 10)
                return "0" + Time.ToString();
            else
                return Time.ToString();
        }

        private string FormatTime(ulong seconds) {
            TimeSpan span = TimeSpan.FromSeconds(seconds);
            if (span.TotalHours < 24) {
                return string.Format("{0}:{1}:{2}", TimeIntToStr(span.Hours), TimeIntToStr(span.Minutes), TimeIntToStr(span.Seconds));
            } else {
                return string.Format("{0}d {1}:{2}:{3}", span.Days, TimeIntToStr(span.Hours), TimeIntToStr(span.Minutes), TimeIntToStr(span.Seconds));
            }
        }

        [HttpGet("{key}")]
        public JsonResult RefreshGlobalActivity(string key)
        {
            var model = new UpdateModel();

            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            var now = DateTime.UtcNow;
            var last_month = now.AddMonths(-1);
            CacheManager cm = CacheManager.Instance;

            var characters = new List<SpecificCharacterActivity>();
            var q = context.ServerSessions
                .Where(c => (c.SessionStart.Year == now.Year && c.SessionStart.Month == now.Month) || (c.SessionStart.Year == last_month.Year && c.SessionStart.Month == last_month.Month))
                .Select(c => new {
                    c.PlayerName,
                    c.SessionStart,
                    SessionGoodDuration = c.ServerSessionsDataConn.Where(x => x.SessionId == c.SessionId && !cm.WastedActivitySystems.Contains(x.Location)).Sum(x => x.Duration)
                })
                .GroupBy(c => new {
                    c.PlayerName,
                    c.SessionStart.Month
                })
                .Select(c => new {
                    c.Key.PlayerName,
                    c.Key.Month,
                    GoodDuration = c.Sum(x => x.SessionGoodDuration)
                })
                .OrderByDescending(c => c.GoodDuration);


            Dictionary<string, ulong> curr_time = new Dictionary<string, ulong>();
            Dictionary<string, ulong> last_time = new Dictionary<string, ulong>();
            foreach (var char_activity in q.ToList()) {
                if (char_activity.Month == now.Month) {
                    curr_time[char_activity.PlayerName] = (ulong) char_activity.GoodDuration;
                } else {
                    last_time[char_activity.PlayerName] = (ulong) char_activity.GoodDuration;
                }
            }

            foreach (KeyValuePair<string, ulong> entry in curr_time) {
                var ca = new SpecificCharacterActivity();
                ca.CharName = entry.Key;
                ca.Current_Time = FormatTime(entry.Value);
                if (last_time.ContainsKey(entry.Key)) {
                    ca.Last_Time = FormatTime(last_time[entry.Key]);
                }
                characters.Add(ca);
            }

            foreach (KeyValuePair<string, ulong> entry in last_time) {
                if (curr_time.ContainsKey(entry.Key)) {
                    continue;
                }
                var ca = new SpecificCharacterActivity();
                ca.CharName = entry.Key;
                ca.Last_Time = FormatTime(entry.Value);
                characters.Add(ca);
            }

            cm.GlobalIndividualActivityCache = characters;
            cm.LastGlobalIndividualActivityCache = DateTime.UtcNow;

            model.Error = "OK";
            logger.LogInformation("Successfully updated global activity report");
            return Json(model);
        }

        [HttpGet("{key}")]
        public JsonResult UpdateFactionSummaryCache(string key)
        {
            var updateModel = new UpdateModel();

            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                updateModel.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(updateModel);
            }

            var model = new FactionSummaryModel();
            var now = DateTime.UtcNow;
            CacheManager cm = CacheManager.Instance;

            logger.LogInformation("Starting GetFactionSummary cache renewal", now);
            model.Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            model.Factions = new List<FactionSummarySingle>();
            
            var start_now = new DateTime(now.Year, now.Month, 1, 0, 0, 0, 0);

            var quarter_start_month = ((int) Math.Floor((double) (now.Month - 1) / 3)) * 3 + 1;
            var start_now_quarter = new DateTime(now.Year, quarter_start_month, 1, 0, 0, 0, 0);

            var start_last = start_now.AddMonths(-1);
            var end_last = new DateTime(start_last.Year, start_last.Month, DateTime.DaysInMonth(start_last.Year, start_last.Month), 23, 59, 59, 999);

            var start_last3 = start_now.AddMonths(-3);

            var Factions = context.ServerFactions.ToList();
            foreach (var faction in Factions)
            {
                var FactionMdl = new FactionSummarySingle();
                FactionMdl.Name = faction.FactionName;
                FactionMdl.Tag = faction.FactionTag;
                FactionMdl.Danger = false;
                FactionMdl.Id = faction.Id;

                ulong curr_time = 0;
                ulong last_time = 0;
                ulong curr_quarter_time = 0;
                ulong last3_time = 0;
                
                //Potentially existing activity records
                var FactionActivity = context.ServerFactionsActivity.Where(c => c.FactionId == faction.Id).ToList();

                // TODO: Code duplication is way out of control here, we really need to move some of this out to a function that can be reused for each of the different periods

                //Get all sessions for the current month
                IQueryable<ServerSessions> factionSessions = context.ServerSessions;
                if (faction.FactionTag == "[TBH]") {
                    factionSessions = factionSessions.Where(c => c.PlayerName.Contains(faction.FactionTag));
                } else {
                    factionSessions = factionSessions.Where(c => c.PlayerName.StartsWith(faction.FactionTag));
                }

                var sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_now && c.SessionStart <= now && c.SessionEnd.HasValue);
                foreach (var item in sessions.ToList())
                {
                    foreach (var system in item.ServerSessionsDataConn)
                    {
                        if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                        {
                            //not wasted
                            curr_time += (ulong)system.Duration;
                        }
                    }
                }

                //Get the sessions of the previous month
                if (FactionActivity.Any(c => c.Stamp == start_last))
                {
                    var activity = FactionActivity.SingleOrDefault(c => c.Stamp == start_last);
                    last_time = activity.Duration;
                }
                else
                {
                    //The values have not yet been precalculated
                    // I feel very hesitant to allow recalculations to be performed here, so I will not do it for now
                    sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_last && c.SessionStart <= end_last && c.SessionEnd.HasValue);
                    foreach (var item in sessions.ToList())
                    {
                        foreach (var system in item.ServerSessionsDataConn)
                        {
                            if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                            {
                                //not wasted
                                last_time += (ulong)system.Duration;
                            }
                        }
                    }
                }

                //Get the sessions of the previous months in this quarter
                for (int i = 0; i < 3; i++) {
                    var start_quarter_thismonth = start_now_quarter.AddMonths(i);
                    var end_quarter_thismonth = new DateTime(start_quarter_thismonth.Year, start_quarter_thismonth.Month, DateTime.DaysInMonth(start_quarter_thismonth.Year, start_quarter_thismonth.Month), 23, 59, 59, 999);
                    if (FactionActivity.Any(c => c.Stamp == start_quarter_thismonth))
                    {
                        var activity = FactionActivity.SingleOrDefault(c => c.Stamp == start_quarter_thismonth);
                        curr_quarter_time += activity.Duration;
                    }
                    else
                    {
                        //The values have not yet been precalculated
                        // I feel very hesitant to allow recalculations to be performed here, so I will not do it for now
                        sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_quarter_thismonth && c.SessionStart <= end_quarter_thismonth && c.SessionEnd.HasValue);
                        foreach (var item in sessions.ToList())
                        {
                            foreach (var system in item.ServerSessionsDataConn)
                            {
                                if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                                {
                                    //not wasted
                                    curr_quarter_time += (ulong)system.Duration;
                                }
                            }
                        }
                    }
                }

                //Get the sessions of the previous 3 month
                for (int i = 0; i < 3; i++) {
                    var start_last3_thismonth = start_last3.AddMonths(i);
                    var end_last3_thismonth = new DateTime(start_last3_thismonth.Year, start_last3_thismonth.Month, DateTime.DaysInMonth(start_last3_thismonth.Year, start_last3_thismonth.Month), 23, 59, 59, 999);
                    if (FactionActivity.Any(c => c.Stamp == start_last3_thismonth))
                    {
                        var activity = FactionActivity.SingleOrDefault(c => c.Stamp == start_last3_thismonth);
                        last3_time += activity.Duration;
                    }
                    else
                    {
                        //The values have not yet been precalculated
                        // I feel very hesitant to allow recalculations to be performed here, so I will not do it for now
                        sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_last3_thismonth && c.SessionStart <= end_last3_thismonth && c.SessionEnd.HasValue);
                        foreach (var item in sessions.ToList())
                        {
                            foreach (var system in item.ServerSessionsDataConn)
                            {
                                if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                                {
                                    //not wasted
                                    last3_time += (ulong)system.Duration;
                                }
                            }
                        }
                    }
                }

                //Compile the data
                FactionMdl.Current_Time = FormatTime(curr_time);
                FactionMdl.Last_Time = FormatTime(last_time);
                FactionMdl.Current_Quarter_Time = FormatTime(curr_quarter_time);
                FactionMdl.Last3_Time = FormatTime(last3_time);

                if (curr_time < cm.Faction_DangerThreshold)
                    FactionMdl.Danger = true;

                model.Factions.Add(FactionMdl);

            }

            cm.FactionGlobalActivityCache = model;
            cm.LastFactionGlobalActivityCache = DateTime.UtcNow;

            logger.LogInformation("Ending GetFactionSummary cache renewal", cm.LastFactionGlobalActivityCache);

            return Json(updateModel);
        }

        private bool isValidKey(string key)
        {
            if (context.ApiKeys.Any(c => c.Key == key && c.Admin == true))
                return true;
            else
                return false;
        }
    }
}
