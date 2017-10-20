using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class GlobalDetailsModel
    {
        public string Error { get; set; }
        public List<SpecificCharacterActivity> Characters { get; set; }
        public int MaxPage { get; set; }
        public string Timestamp { get; set; }
    }

    public class SpecificCharacterActivity
    {
        public string CharName { get; set; }
        public string Current_Time { get; set; }
        public string Last_Time { get; set; }
    }
}
