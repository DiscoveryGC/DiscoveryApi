using System;
using System.Collections.Generic;

namespace DiscoveryApi.Models
{
    public partial class ServerSessions
    {
        public int SessionId { get; set; }
        public string PlayerId { get; set; }
        public int PlayerLagAvg { get; set; }
        public string PlayerLastShip { get; set; }
        public int PlayerLossAvg { get; set; }
        public string PlayerName { get; set; }
        public int PlayerPingAvg { get; set; }
        public DateTime? SessionEnd { get; set; }
        public string SessionIp { get; set; }
        public DateTime SessionStart { get; set; }
    }
}
