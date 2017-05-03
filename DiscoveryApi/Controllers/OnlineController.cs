using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiscoveryApi.Models;
using DiscoveryApi.DAL;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiscoveryApi.Controllers
{
    [Route("api/[controller]")]
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
        [HttpGet, Route("{key}")]
        public JsonResult Index(string key)
        {
            var model = new PlayersOnline();
            if (!isValidKey(key))
            {
                logger.LogWarning("Illegal access attempt with key: " + key, ", ip: " + HttpContext.Request.Host);
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

            var activity = context.ServerPlayercounts.ToList();
            return Json(activity);

            //var myFactions = context.ServerFactions.ToList();
            //return Json(myFactions.ToList());
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
