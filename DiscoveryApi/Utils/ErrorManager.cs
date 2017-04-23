using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DiscoveryApi.Utils
{
    public class ErrorManager
    {
        private readonly string LogPath;
        public ErrorManager(IHostingEnvironment hostingEnvironment)
        {
            LogPath = hostingEnvironment.ContentRootPath + "/errors.txt";
        }

        public void LogError(string Error)
        {
            var file = File.AppendText(LogPath);
            string format = String.Format("[{0}] {1}", DateTime.Now.ToString(), Error);
            file.WriteLine(format);
        }
    }
}
