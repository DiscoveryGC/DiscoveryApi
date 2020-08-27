using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscoveryApi.Models;

namespace DiscoveryApi.Utils
{
    public struct FactionCache
    {
        public DateTime LastCache { get; set; }
        public FactionDetailsModel Cache { get; set; }
    }

    /// <summary>
    /// This is a unique cache class shared by the entire application at any time.
    /// </summary>
    public sealed class CacheManager
    {
        static readonly CacheManager _instance = new CacheManager();
        public static CacheManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public DateTime LastUpdate = new DateTime(0);
        public int MaxRetry = 10;
        public int Retry = 0;
        //Current Faction Danger Threshold: 24h
        public ulong Faction_DangerThreshold = 86400;

        //Available caches
        public PlayersOnline<PlayerOnlineSingle> PlayerOnlineCache;
        public DateTime LastPlayerOnlineCache = new DateTime(0);
        public int LastPlayerOnlineCacheDuration = 60;
        
        public FactionSummaryModel FactionGlobalActivityCache;
        public DateTime LastFactionGlobalActivityCache = new DateTime(0);

        public List<SpecificCharacterActivity> GlobalIndividualActivityCache;
        public DateTime LastGlobalIndividualActivityCache = new DateTime(0);
        public int GlobalIndividualCacheDuration = 10800;

        public Dictionary<string, FactionCache> FactionIndividualActivityCache = new Dictionary<string, FactionCache>();
        public int FactionIndividualCacheDuration = 900;

        public List<string> WastedActivitySystems = new List<string>() { "LI06", "IW09" };

        public CacheManager()
        {
 
        }
    }
}
