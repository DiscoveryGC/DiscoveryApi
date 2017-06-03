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
            var model = new PlayersOnline();
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
                    player.Faction = "Unknown";
                    player.Name = item.PlayerName;

                    TimeSpan span = now.Subtract(item.SessionStart);
                    if (span.Hours > 0)
                        player.Time = span.Hours.ToString() + "h" + (span.Minutes < 10 ? "0" : "") + span.Minutes.ToString();
                    else
                        player.Time = span.Minutes.ToString() + "m";

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

                    model.Players.Add(player);
                }

                cm.PlayerOnlineCache = JsonConvert.SerializeObject(model);
                cm.LastPlayerOnlineCache = DateTime.UtcNow;
            }

            return Json(cm.PlayerOnlineCache);
        }

        [HttpGet("{key}")]
        public JsonResult GetFactionSummary(string key)
        {
            var model = new FactionSummaryModel();
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            CacheManager cm = CacheManager.Instance;

            //Check if we have to renew the cache
            var now = DateTime.UtcNow;
            if (cm.LastFactionGlobalActivityCache.AddSeconds(cm.FactionGlobalActivityDuration) < now)
            {
                model.Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                model.Factions = new List<FactionSummarySingle>();

                var Factions = context.ServerFactions.ToList();

                foreach (var item in Factions)
                {
                    //Get all sessions independent of name, we don't care for individual players
                }

                cm.FactionGlobalActivityCache = JsonConvert.SerializeObject(model);
                cm.LastFactionGlobalActivityCache = DateTime.UtcNow;
            }

            return Json(cm.FactionGlobalActivityCache);
        }

        private bool isValidKey(string key)
        {
            if (context.ApiKeys.Any(c => c.Key == key))
                return true;
            else
                return false;
        }
    }
}
