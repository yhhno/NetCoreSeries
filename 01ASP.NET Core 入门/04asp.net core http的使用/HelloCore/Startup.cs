using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

namespace HelloCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,IConfiguration configuration,IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //app.UseRouter();

            applicationLifetime.ApplicationStarted.Register(() => {
                //application 启动时的 定义的动作.
                Console.WriteLine("start;");
            });

            applicationLifetime.ApplicationStopping.Register(() => {
                Console.WriteLine("stopping;");
            });
            applicationLifetime.ApplicationStopped.Register(() => {
                Console.WriteLine("stop;");
                Console.ReadLine();
            });


            app.Run(async (context) =>
            {
                //输出配置
                await context.Response.WriteAsync(configuration["ConnectionStrings:DefaultConnection"]);

                //输出命令行参数
                await context.Response.WriteAsync($"name={configuration["name"]}");

                //25 输出env的内容

                await context.Response.WriteAsync($"ContentRootPath={env.ContentRootPath}");//项目文件目录
                await context.Response.WriteAsync($"EnvironmentName={env.EnvironmentName}");
                await context.Response.WriteAsync($"WebRootPath={env.WebRootPath}");//web根目录,项目默认会创建一个wwwroot文件夹,所有要把静态文件等放在此文件夹下.
                await context.Response.WriteAsync($"ApplicationName={env.ApplicationName}");
              

                //默认代码
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
