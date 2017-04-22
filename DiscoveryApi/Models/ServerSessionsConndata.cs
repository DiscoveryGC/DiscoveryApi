using System;
using System.Collections.Generic;

namespace DiscoveryApi.Models
{
    public partial class ServerSessionsConndata
    {
        public int SessionId { get; set; }
        public DateTime SessionRec { get; set; }
        public int? PlayerLag { get; set; }
        public int? PlayerLoss { get; set; }
        public int? PlayerPing { get; set; }
        public string PlayerShip { get; set; }
        public string PlayerSystem { get; set; }
    }
}
