using System;
using System.Collections.Generic;

namespace DiscoveryApi.Models
{
    public partial class ServerFactions
    {
        public string FactionTag { get; set; }
        public DateTime FactionAdded { get; set; }
        public string FactionName { get; set; }
        public string ItemEquipId { get; set; }
        public bool Warned { get; set; }
    }
}
