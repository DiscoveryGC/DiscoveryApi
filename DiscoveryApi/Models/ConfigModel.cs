using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoveryApi.Models
{
    public class ConfigModel
    {
        public ApiConfigModel ApiSettings { get; set; }
    }

    public class ApiConfigModel
    {
        public string JsonLocation { get; set; }
    }
}
