using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


using MvcCookieAuthSampleAddUI.Data;

namespace MvcCookieAuthSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //启动后第一件事情做，，或者启动之前先做 ，两个时候任意都可以，先把数据库跑一遍。
            BuildWebHost(args)
                .MigrateDbContext<ApplicationDbContext>((context, services) =>
                {
                    //我们在这里做我们的初始化
                    new ApplicationDbContextSeed().SeedAsync(context, services)
                    .Wait() ;//异步方法的同步执行。
                })
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
