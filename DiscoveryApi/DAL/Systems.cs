using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class Systems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int RegionId { get; set; }

        public virtual Regions Region { get; set; }
    }
}
