using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class UpdateModel
    {
        public string Error { get; set; }
    }

    public class UpdateResponseRawModel
    {
        public string Timestamp { get; set; }
        public Dictionary<string, UpdateResponseRawPlayerModel> Players { get; set; }
    }

    public class UpdateResponseRawPlayerModel
    {
        public string System { get; set; }
        public string Ip { get; set; }
        public string Ship { get; set; }
        public string Id { get; set; }
        public int Ping { get; set; }
        public int Loss { get; set; }
        public int Lag { get; set; }
    }

    public class UpdateResponseCloakRawModel
    {
        public Dictionary<string, UpdateResponseCloakRawPlayerModel> Players { get; set; }
    }

    public class UpdateResponseRawPlayerModel
    {
        public string System { get; set; }
    }

    public class UpdateResponseModel
    {
        public DateTime Timestamp { get; set; }
        public List<UpdateResponsePlayerModel> Players { get; set; }
    }

    public class UpdateResponsePlayerModel
    {
        public string Name { get; set;}
        public string System { get; set; }
        public string ObfuscatedSystem { get; set; }
        public string Ip { get; set; }
        public string Ship { get; set; }
        public string Id { get; set; }
        public int Ping { get; set; }
        public int Loss { get; set; }
        public int Lag { get; set; }
    }

    public struct UpdatePlayerCountStruct
    {
        public DateTime Date { get; set; }
        public short Count { get; set; }
    }
}
