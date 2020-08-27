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
        public PlayersOnline<PlayerOnlineSingle> PlayerOnlineCache { get; set; }
        public DateTime LastPlayerOnlineCache { get; set; }

        public FactionSummaryModel FactionGlobalActivityCache { get; set; }
        public DateTime LastFactionGlobalActivityCache { get; set; }

        public Dictionary<string, FactionCache> FactionIndividualActivityCache { get; set; }
    }
}
