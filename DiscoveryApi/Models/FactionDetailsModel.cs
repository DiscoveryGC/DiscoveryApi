using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class FactionDetailsModel
    {
        public string Error { get; set; }
        public Dictionary<string, CharacterActivity> Characters { get; set; }
        public string Timestamp { get; set; }
    }

    public class CharacterActivity
    {
        public string Current_Time { get; set; }
        public string Last_Time { get; set; }
    }
}
