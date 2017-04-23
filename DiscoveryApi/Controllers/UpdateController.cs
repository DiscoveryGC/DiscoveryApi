using System;
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
using System.Linq;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiscoveryApi.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly apiContext context;
        private readonly IOptions<ConfigModel> config;
        public UpdateController(apiContext _context, IOptions<ConfigModel> _config)
        {
            context = _context;
            config = _config;
        }

        // GET: /<controller>/
        [HttpGet, Route("{key}")]
        public JsonResult Index(string key)
        {
            var model = new UpdateModel();

            if (!isValidKey(key))
            {
                model.Error = Ressources.ApiResource.UnauthorizedAccess;
                return Json(model);
            }

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
            }

            //We are done receiving the data, we can now update our database
            //We are more likely to have a successful request
            
            if (Success)
            {
                List<string> ProcessedPlayers = new List<string>();
                var ActivePlayers = context.ServerSessions.Include(c => c.ServerSessionsDataConn).Where(c => !c.SessionEnd.HasValue).ToList();
                //First, we're going to see if we have sessions to end or update
                foreach (var item in ActivePlayers)
                {
                    var last_system = item.ServerSessionsDataConn.LastOrDefault();
                    TimeSpan Diff = Result.Timestamp - last_system.Stamp;

                    //Is the player still online?
                    if (!Result.Players.Any(c => c.Name == item.PlayerName))
                    {
                        //Nope, end the session and compile stats
                        item.SessionEnd = DateTime.Now;
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
                        if (last_system.Location == PlayerInfo.System)
                        {
                            //The player hasn't changed systems. Update the current information.
                            last_system.Lag = (last_system.Lag + PlayerInfo.Lag) / 2;
                            last_system.Loss = (last_system.Loss + PlayerInfo.Loss) / 2;
                            last_system.Ping = (last_system.Ping + PlayerInfo.Ping) / 2;
                            last_system.Ship = "test";
                            last_system.Duration += (int)Diff.TotalSeconds;
                            last_system.Stamp = Result.Timestamp;
                        }
                        else
                        {
                            //The player has changed systems
                            //Update the duration of the previous system
                            last_system.Duration += (int)Diff.TotalSeconds;

                            //Create a new system entry
                            var system = new ServerSessionsDataConn();
                            system.SessionId = item.SessionId;
                            system.Stamp = Result.Timestamp;
                            system.Ship = PlayerInfo.Ship;
                            system.Location = PlayerInfo.System;
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
                    context.ServerSessions.Add(Session);

                    Session.PlayerName = item.Name;
                    Session.PlayerId = item.Id;
                    Session.SessionIp = item.Ip;
                    Session.SessionStart = Result.Timestamp;
                    Session.PlayerLagAvg = 0;
                    Session.PlayerLossAvg = 0;
                    Session.PlayerPingAvg = 0;
                    Session.PlayerLastShip = item.Ship;
                    //We do not compile stats here !!!

                    var system = new ServerSessionsDataConn();
                    system.Session = Session;
                    system.Stamp = Result.Timestamp;
                    system.Duration = 0;
                    system.Location = item.System;
                    system.Ship = item.Ship;
                    system.Lag = item.Lag;
                    system.Loss = item.Loss;
                    system.Ping = item.Ping;

                    Session.ServerSessionsDataConn.Add(system);                   
                }

                context.SaveChanges();
                model.Error = "OK";
                return Json(model);
            }
            else
            {
                //Has the request failed? If so, end all current sessions.
                var ActivePlayers = context.ServerSessions.Where(c => !c.SessionEnd.HasValue).ToList();
                foreach (var item in ActivePlayers)
                {
                    item.SessionEnd = DateTime.Now;
                }

                context.SaveChanges();
                model.Error = Ressources.ApiResource.UpdateRequestFailed;
                return Json(model);
            }    
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
