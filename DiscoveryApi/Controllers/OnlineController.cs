using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiscoveryApi.Models;
using DiscoveryApi.DAL;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiscoveryApi.Controllers
{
    [Route("api/[controller]")]
    public class OnlineController : Controller
    {
        private readonly apiContext context;
        public OnlineController(apiContext _context)
        {
            context = _context;
        }

        // GET: /<controller>/
        [HttpGet]
        public JsonResult Index()
        {
            var myFactions = context.ServerFactions.ToList();
            return Json(myFactions.ToList());
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
