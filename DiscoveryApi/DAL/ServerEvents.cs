using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class ServerEvents
    {
        public int Id { get; set; }
        public int CurrentCount { get; set; }
        public string EventDescription { get; set; }
        public string EventName { get; set; }
        public int ExpectedCount { get; set; }
        public bool IsActive { get; set; }
        public int Type { get; set; }
    }
}
