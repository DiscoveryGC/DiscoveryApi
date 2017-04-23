using System;
using System.Collections.Generic;

namespace DiscoveryApi.DAL
{
    public partial class ApiKeys
    {
        public int Id { get; set; }
        public bool Admin { get; set; }
        public string Key { get; set; }
        public string Owner { get; set; }
    }
}
