using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelloCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                //把外部的配置信息,加入config
                config.AddJsonFile("settings.json");
                //也可以把命令行的参数加入 config.
                config.AddCommandLine(args);
            })
            //.UseUrls("http://localhost:5001")
                .UseStartup<Startup>()
                .Build();
    }
}
