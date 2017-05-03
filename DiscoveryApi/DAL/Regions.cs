using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class Regions
    {
        public Regions()
        {
            Systems = new HashSet<Systems>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Systems> Systems { get; set; }
    }
}
