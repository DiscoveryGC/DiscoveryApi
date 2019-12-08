using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class PlayersOnline<T>
    {
        public string Error { get; set; }
        public List<T> Players { get; set; }
        public string Timestamp { get; set; }
    }

    public class PlayerOnlineSingle
    {
        public string Time { get; set;}
        public string Name { get; set; }
        public string System { get; set; }
        public string Region { get; set; }
    }

    public class PlayerOnlineAdmin
    {
        public string Name { get; set; }
        public string System { get; set; }
        public string Region { get; set; }
        public string ID { get; set; }
        public string Ship { get; set; }
        public string IP { get; set; }
        public int Ping { get; set; }
    }
}
