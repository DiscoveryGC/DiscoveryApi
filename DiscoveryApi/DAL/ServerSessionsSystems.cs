using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class ServerSessionsSystems
    {
        public int SessionId { get; set; }
        public string SystemId { get; set; }
        public DateTime VisitDate { get; set; }
        public int VisitDuration { get; set; }
    }
}
