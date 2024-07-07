using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class ServerSessionsDataConn
    {
        public int Id { get; set; }
        public int Duration { get; set; }
        public int Lag { get; set; }
        public string Location { get; set; }
        public string ObfuscatedLocation { get; set; }
        public int Loss { get; set; }
        public int Ping { get; set; }
        public int SessionId { get; set; }
        public string Ship { get; set; }
        public DateTime Stamp { get; set; }

        public virtual ServerSessions Session { get; set; }
    }
}
