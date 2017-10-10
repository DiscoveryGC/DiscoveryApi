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

namespace DiscoveryApi.Controllers
{
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly apiContext context;
        public AdminController(apiContext _context)
        {
            context = _context;
        }

        [HttpGet("{key}")]
        public IActionResult GetCache(string key)
        {
            if (!isValidKey(key))
                return View("Cat");

            var model = new CacheDisplayModel();
            CacheManager cm = CacheManager.Instance;
            model.LastUpdate = cm.LastUpdate;
            model.LastPlayerOnlineCache = cm.LastPlayerOnlineCache;
            model.LastFactionGlobalActivityCache = cm.LastFactionGlobalActivityCache;

            model.PlayerOnlineCache = cm.PlayerOnlineCache != null ? JValue.Parse(cm.PlayerOnlineCache).ToString(Formatting.Indented) : "";
            model.FactionGlobalActivityCache = cm.FactionGlobalActivityCache != null ? JValue.Parse(cm.FactionGlobalActivityCache).ToString(Formatting.Indented) : "";
            model.FactionIndividualActivityCache = cm.FactionIndividualActivityCache;
            model.RetryCount = cm.Retry;
            model.RetryMax = cm.MaxRetry;

            return View(model);
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