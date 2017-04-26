using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Utils
{
    public struct FactionCache
    {
        public string Tag { get; set; }
        public DateTime LastCache { get; set; }
        public string Cache { get; set; }
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

        public DateTime LastUpdate;

        //Available caches
        public string PlayerOnlineCache;
        public DateTime LastPlayerOnlineCache;
        public const int LastPlayerOnlineCacheDuration = 60;
        
        public string PlayerCountCache;
        public DateTime LastPlayerCountCache;
        public const int LastPlayerCountCacheDuration = 900;

        public string FactionGlobalActivityCache;
        public DateTime LastFactionGlobalActivityCache;
        public const int FactionGlobalActivityDuration = 900;

        public List<FactionCache> FactionIndividualActivityCache;
        public const int FactionIndividualCacheDuration = 900;

        public CacheManager()
        {
            //Initialize the cache objects
            FactionIndividualActivityCache = new List<FactionCache>();
            LastUpdate = new DateTime(0);
        }
    }
}
