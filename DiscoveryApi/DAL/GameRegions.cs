using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class GameRegions
    {
        public GameRegions()
        {
            GameSystems = new HashSet<GameSystems>();
        }

        public int Id { get; set; }
        public string Fullname { get; set; }

        public virtual ICollection<GameSystems> GameSystems { get; set; }
    }
}
