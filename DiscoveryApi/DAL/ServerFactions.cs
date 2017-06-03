using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class ServerFactions
    {
        public ServerFactions()
        {
            ServerFactionsActivity = new HashSet<ServerFactionsActivity>();
        }

        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime FactionAdded { get; set; }
        public string FactionName { get; set; }
        public string FactionTag { get; set; }
        public bool IdTracking { get; set; }
        public string ItemEquipId { get; set; }
        public bool Warned { get; set; }

        public virtual ICollection<ServerFactionsActivity> ServerFactionsActivity { get; set; }
    }
}
