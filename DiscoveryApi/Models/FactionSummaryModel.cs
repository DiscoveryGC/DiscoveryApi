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
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Current_Time { get; set; }
        public string Last_Time { get; set; }
        public string Current_Quarter_Time { get; set; }
        public string Last3_Time { get; set; }
        public bool Danger { get; set; }
    }
}
