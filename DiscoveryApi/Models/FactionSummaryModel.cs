using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class FactionSummaryModel
    {
        public string Error { get; set; }
        public List<FactionSummarySingle> Factions { get; set; }
        public string Timestamp { get; set; }
    }

    public class FactionSummarySingle
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public bool Danger { get; set; }
        public int Performance { get; set; }
    }
}
