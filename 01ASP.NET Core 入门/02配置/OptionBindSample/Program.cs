using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OptionBindSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        //webhost启动的时候,默认会把appsettings.json文件读到configuration中去,通过CreateDefaultBuilder方法
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config => {
                config.AddJsonFile("appsettings.json", false, false);
            })//覆盖热更新
                .UseStartup<Startup>()
                .Build();
    }
}
