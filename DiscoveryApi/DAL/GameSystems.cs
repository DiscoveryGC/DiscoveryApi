using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class GameSystems
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Nickname { get; set; }
        public int RegionId { get; set; }

        public virtual GameRegions Region { get; set; }
    }
}
