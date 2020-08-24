using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiscoveryApi.Models;
using DiscoveryApi.DAL;
using Microsoft.Extensions.Logging;
using DiscoveryApi.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiscoveryApi.Controllers
{
    [Area("api"), Route("[area]/[controller]/[action]")]
    public class OnlineController : Controller
    {
        private readonly ILogger<OnlineController> logger;
        private readonly apiContext context;
        public OnlineController(apiContext _context, ILogger<OnlineController> _logger)
        {
            context = _context;
            logger = _logger;
        }

        // GET: /<controller>/
        [HttpGet("{key}")]
        public JsonResult GetPlayers(string key)
        {
            var model = new PlayersOnline<PlayerOnlineSingle>();
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            CacheManager cm = CacheManager.Instance;

            //Check if we have to renew the cache
            var now = DateTime.UtcNow;

            if (cm.LastPlayerOnlineCache.AddSeconds(cm.LastPlayerOnlineCacheDuration) < now)
            {
                model.Timestamp = cm.LastUpdate.ToString("yyyy-MM-ddTHH:mm:ss");
                model.Players = new List<PlayerOnlineSingle>();
                var players = context.ServerSessions.Include(c => c.ServerSessionsDataConn).Where(c => !c.SessionEnd.HasValue).ToList();
                var systems = context.Systems.ToList();
                var regions = context.Regions.ToList();
                //We can add the factions later

                foreach (var item in players)
                {
                    var player = new PlayerOnlineSingle();
                    player.Name = item.PlayerName;

                    TimeSpan span = now.Subtract(item.SessionStart);
                    if (span.Hours > 0)
                        player.Time = span.Hours.ToString() + "h" + (span.Minutes < 10 ? "0" : "") + span.Minutes.ToString();
                    else
                        player.Time = span.Minutes.ToString() + "m";

                    //Last location data
                    var last_system = item.ServerSessionsDataConn.OrderByDescending(c => c.Stamp).FirstOrDefault();
                    if (last_system != null)
                    {
                        var system = systems.SingleOrDefault(c => c.Nickname == last_system.Location.ToUpper()) ?? null;

                        //This is the always expected scenario
                        player.System = system != null ? system.Name : "Unknown";
                        player.Region = system != null ? regions.SingleOrDefault(c => c.Id == system.RegionId)?.Name ?? "Unknown" : "Unknown";
                    }
                    else
                    {
                        //But better be safe than sorry
                        player.System = "ERROR";
                        player.Region = "ERROR";
                    }

                    model.Players.Add(player);
                }

                cm.PlayerOnlineCache = JsonConvert.SerializeObject(model);
                cm.LastPlayerOnlineCache = DateTime.UtcNow;
            }

            return Json(cm.PlayerOnlineCache);
        }

        [HttpGet("{key}/{page}")]
        public JsonResult GetAllPlayers(string key, string page)
        {
            var model = new GlobalDetailsModel();

            var pageInt = -1;
            if (!Int32.TryParse(page, out pageInt))
            {
                model.Error = Ressources.ApiResource.InvalidPageParameter;
                return Json(model);
            }

            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            CacheManager cm = CacheManager.Instance;
            if (cm.GlobalIndividualActivityCache == null)
            {
                model.Error = Ressources.ApiResource.DataNotYetPopulated;
                return Json(JsonConvert.SerializeObject(model));
            }

            var CHARACTERS_PER_PAGE = 1000;
            if (pageInt < 1 || (pageInt - 1) * CHARACTERS_PER_PAGE >= cm.GlobalIndividualActivityCache.Count)
            {
                model.Error = Ressources.ApiResource.PageParameterOutOfBounds;
                return Json(JsonConvert.SerializeObject(model));
            }

            model.Timestamp = cm.LastGlobalIndividualActivityCache.ToString("yyyy-MM-ddTHH:mm:ss");
            int count = Math.Min(cm.GlobalIndividualActivityCache.Count - (pageInt - 1) * CHARACTERS_PER_PAGE, CHARACTERS_PER_PAGE);
            model.Characters = cm.GlobalIndividualActivityCache.GetRange((pageInt - 1) * CHARACTERS_PER_PAGE, count);
            model.MaxPage = (int) Math.Ceiling((double) cm.GlobalIndividualActivityCache.Count / CHARACTERS_PER_PAGE);
            return Json(JsonConvert.SerializeObject(model));
        }

        // GET: /<controller>/
        [HttpGet("{key}")]
        public JsonResult AdminGetPlayers(string key)
        {
            var model = new PlayersOnline<PlayerOnlineAdmin>();
            if (!isValidKey(key, true))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            model.Timestamp = CacheManager.Instance.LastUpdate.ToString("yyyy-MM-ddTHH:mm:ss");
            model.Players = new List<PlayerOnlineAdmin>();
            var players = context.ServerSessions.Include(c => c.ServerSessionsDataConn).Where(c => !c.SessionEnd.HasValue).ToList();
            var systems = context.Systems.ToList();
            var regions = context.Regions.ToList();

            foreach (var item in players)
            {
                var player = new PlayerOnlineAdmin();
                player.Name = item.PlayerName;

                //Last location data
                var last_system = item.ServerSessionsDataConn.LastOrDefault();
                if (last_system != null)
                {
                    var system = systems.SingleOrDefault(c => c.Nickname == last_system.Location.ToUpper()) ?? null;

                    //This is the always expected scenario
                    player.System = system != null ? system.Name : "Unknown";
                    player.Region = system != null ? regions.SingleOrDefault(c => c.Id == system.RegionId)?.Name ?? "Unknown" : "Unknown";
                    player.Ping = last_system.Ping;
                }
                else
                {
                    //But better be safe than sorry
                    player.Ping = 0;
                    player.System = "ERROR";
                    player.Region = "ERROR";
                }

                player.ID = item.PlayerId;
                player.Ship = item.PlayerLastShip;
                player.IP = item.SessionIp;

                model.Players.Add(player);
            }

            return Json(JsonConvert.SerializeObject(model));
        }

        [HttpGet("{tag}/{key}")]
        public JsonResult GetFactionDetails(string tag, string key)
        {
            var model = new FactionDetailsModel();
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            // Kestrel is retarded and decodes most stuff, just not the forward slash character which is used in Auxesia's tag of A/)-
            tag = tag.Replace("%2F", "/");

            var now = DateTime.UtcNow;
            CacheManager cm = CacheManager.Instance;

            var factions = context.ServerFactions.Where(c => c.FactionTag == tag);
            if (factions.Count() == 0) {
                model.Error = Ressources.ApiResource.FactionNotFound;
                return Json(model);
            }
            var faction = factions.First();

            // Check if we have to renew the cache
            if (!cm.FactionIndividualActivityCache.ContainsKey(tag) || cm.FactionIndividualActivityCache[tag].LastCache.AddSeconds(cm.FactionIndividualCacheDuration) < now)
            {
                var start_now = new DateTime(now.Year, now.Month, 1, 0, 0, 0, 0);

                var quarter_start_month = ((int) Math.Floor((double) (now.Month - 1) / 3)) * 3 + 1;
                var start_now_quarter = new DateTime(now.Year, quarter_start_month, 1, 0, 0, 0, 0);

                var start_last = start_now.AddMonths(-1);
                var end_last = new DateTime(start_last.Year, start_last.Month, DateTime.DaysInMonth(start_last.Year, start_last.Month), 23, 59, 59, 999);

                var start_last3 = start_now.AddMonths(-3);

                model.Timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss");
                model.Characters = new Dictionary<string, CharacterActivity>();

                Dictionary<string, ulong> curr_time = new Dictionary<string, ulong>();
                Dictionary<string, ulong> last_time = new Dictionary<string, ulong>();
                Dictionary<string, ulong> curr_quarter_time = new Dictionary<string, ulong>();
                Dictionary<string, ulong> last3_time = new Dictionary<string, ulong>();

                //Get all sessions for the current month
                IQueryable<ServerSessions> factionSessions = context.ServerSessions;
                if (faction.FactionTag == "L\\-") {
                    factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE 'L\\\\\\\\-%'");
                } else if (faction.FactionTag == "|\\/|-") {
                    factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '|\\\\\\\\/|-%'");
                } else if (faction.FactionTag == "\\*/~") {
                    factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '\\\\\\\\*/~%'");
                } else if (faction.FactionTag == "(\\^/)") {
                    factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '(\\\\\\\\^/)%'");
                } else if (faction.FactionTag == "/+\\-") {
                    factionSessions = factionSessions.FromSql("SELECT * FROM server_sessions WHERE player_name LIKE '/+\\\\\\\\-%'");
                } else if (faction.FactionTag == "[TBH]" || faction.FactionTag == "|Aoi" || faction.FactionTag == "Reaver") {
                    factionSessions = factionSessions.Where(c => c.PlayerName.Contains(faction.FactionTag));
                } else {
                    factionSessions = factionSessions.Where(c => c.PlayerName.StartsWith(faction.FactionTag));
                }

                var sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_now && c.SessionStart <= now && c.SessionEnd.HasValue);
                foreach (var session in sessions.ToList())
                {
                    model.Characters[session.PlayerName] = new CharacterActivity();
                    if (!curr_time.ContainsKey(session.PlayerName)) {
                        curr_time[session.PlayerName] = 0;
                    }
                    foreach (var system in session.ServerSessionsDataConn)
                    {
                        if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                        {
                            curr_time[session.PlayerName] += (ulong)system.Duration;
                        }
                    }
                }

                // Get all sessions for last month
                sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_last && c.SessionStart <= end_last && c.SessionEnd.HasValue);
                foreach (var session in sessions.ToList())
                {
                    model.Characters[session.PlayerName] = new CharacterActivity();
                    if (!last_time.ContainsKey(session.PlayerName)) {
                        last_time[session.PlayerName] = 0;
                    }
                    foreach (var system in session.ServerSessionsDataConn)
                    {
                        if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                        {
                            last_time[session.PlayerName] += (ulong)system.Duration;
                        }
                    }
                }

                // Get all sessions for the current quarter
                sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_now_quarter && c.SessionStart <= now && c.SessionEnd.HasValue);
                foreach (var session in sessions.ToList())
                {
                    model.Characters[session.PlayerName] = new CharacterActivity();
                    if (!curr_quarter_time.ContainsKey(session.PlayerName)) {
                        curr_quarter_time[session.PlayerName] = 0;
                    }
                    foreach (var system in session.ServerSessionsDataConn)
                    {
                        if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                        {
                            curr_quarter_time[session.PlayerName] += (ulong)system.Duration;
                        }
                    }
                }

                // Get all sessions for the last three months
                sessions = factionSessions.Include(c => c.ServerSessionsDataConn).Where(c => c.SessionStart >= start_last3 && c.SessionStart <= end_last && c.SessionEnd.HasValue);
                foreach (var session in sessions.ToList())
                {
                    model.Characters[session.PlayerName] = new CharacterActivity();
                    if (!last3_time.ContainsKey(session.PlayerName)) {
                        last3_time[session.PlayerName] = 0;
                    }
                    foreach (var system in session.ServerSessionsDataConn)
                    {
                        if (!cm.WastedActivitySystems.Contains(system.Location.ToUpper()))
                        {
                            last3_time[session.PlayerName] += (ulong)system.Duration;
                        }
                    }
                }

                foreach (KeyValuePair<string, CharacterActivity> entry in model.Characters)
                {
                    //Compile the data
                    ulong curr_seconds = 0;
                    ulong last_seconds = 0;
                    ulong curr_quarter_seconds = 0;
                    ulong last3_seconds = 0;
                    if (curr_time.ContainsKey(entry.Key)) {
                        curr_seconds = curr_time[entry.Key];
                    }
                    if (last_time.ContainsKey(entry.Key)) {
                        last_seconds = last_time[entry.Key];
                    }
                    if (curr_quarter_time.ContainsKey(entry.Key)) {
                        curr_quarter_seconds = curr_quarter_time[entry.Key];
                    }
                    if (last3_time.ContainsKey(entry.Key)) {
                        last3_seconds = last3_time[entry.Key];
                    }
                    entry.Value.Current_Time = FormatTime(curr_seconds);
                    entry.Value.Last_Time = FormatTime(last_seconds);
                    entry.Value.Current_Quarter_Time = FormatTime(curr_quarter_seconds);
                    entry.Value.Last3_Time = FormatTime(last3_seconds);
                }

                var cache = new FactionCache();
                cache.Cache = JsonConvert.SerializeObject(model);
                cache.LastCache = DateTime.UtcNow;
                cm.FactionIndividualActivityCache[tag] = cache;
            }

            return Json(cm.FactionIndividualActivityCache[tag].Cache);
        }

        [HttpGet("{key}")]
        public JsonResult GetFactionSummary(string key)
        {
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                var model = new FactionSummaryModel();
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            return Json(CacheManager.Instance.FactionGlobalActivityCache);
        }

        private bool isValidKey(string key, bool requireAdmin = false)
        {
            if (context.ApiKeys.Any(c => c.Key == key && (!requireAdmin || c.Admin == true)))
                return true;
            else
                return false;
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
    }
}
