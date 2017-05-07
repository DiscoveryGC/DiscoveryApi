using DiscoveryApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class CacheDisplayModel
    {
        public DateTime LastUpdate { get; set; }
        public int RetryCount { get; set; }
        public int RetryMax { get; set; }

        //Available caches
        public string PlayerOnlineCache { get; set; }
        public DateTime LastPlayerOnlineCache { get; set; }

        public string PlayerCountCache { get; set; }
        public DateTime LastPlayerCountCache { get; set; }

        public string FactionGlobalActivityCache { get; set; }
        public DateTime LastFactionGlobalActivityCache { get; set; }

        //public List<FactionCache> FactionIndividualActivityCache;
    }
}
