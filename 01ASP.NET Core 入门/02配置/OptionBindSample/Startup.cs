using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace OptionBindSample
{
    public class Startup
    {

        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)//通过依赖注入,把外部的configuration注入到Startup中,这样在startup内部就是获取外部的configuration
        {
            Configuration = configuration;
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //13
            services.Configure<Class>(Configuration);//把Class注册下,类似于bind的效果
            //13
            services.AddMvc();//把mvc添加进来,这是依赖注入的配置
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //13 启用mvc默认路由, 这样我们就把mvc这个中间件添加到应用程序中
            app.UseMvcWithDefaultRoute();




            //任务12内容,在mvc中,需要将此注销,因为此处会把mvc的管道替代,只输出注释的代码内容
            //app.Run(async (context) =>
            //{
            //    ////Configuration.Bind()// bind方法的参数是一个object实体,将外部的配置信息和内部的实体映射起来

            //    var myclass=new Class();
            //    Configuration.Bind(myclass);//将appsettings.json中的配置信息和class类映射起来.


            //    await context.Response.WriteAsync($"ClassNo:{myclass.ClassNo}");
            //    await context.Response.WriteAsync($"ClassDesc:{myclass.ClassDesc}");
            //    await context.Response.WriteAsync($"StudenCount:{myclass.Students.Count}");
            //});
        }
    }
}
