using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            APIComm.Rigs = new ConcurrentDictionary<string, RigStatus>();
            APIComm.Init();
            //DirComm.Start();
            CreateWebHostBuilder(args).Build().Run();
            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                
                .UseStartup<Startup>()
                .UseUrls($"http://0.0.0.0:5888");
    }
}
