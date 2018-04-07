using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Middleware组成管道实践
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //30
            services.AddRouting();//这个是routing依赖注入的配置
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app是用来构建管道的,用use run map等方法来构造

            //30
            app.UseRouter(builder =>//看的不认真,看错了.,错了以后查找问题不得力.
            {
                builder.MapGet("action", async context =>//RequestDelegate可以处理这个请求.
                {
                    await context.Response.WriteAsync("this is a  aciton");
                });
            });


            //RequestDelegate handler = async context => { await context.Response.WriteAsync("this is a  acitons"); };
            RequestDelegate handler =  context =>  context.Response.WriteAsync("this is a  acitons"); 
            var router = new Route(
                new RouteHandler(handler),
                 "actions",
                 app.ApplicationServices.GetRequiredService<IInlineConstraintResolver>()
                );

            app.UseRouter(router);



            app.UseMvc();


            //public static void Run(this IApplicationBuilder app, RequestDelegate handler); 有啥问题
            app.Map("/task", taskapp =>//此时taskapp会重新启用一个IApplicationbuilder
            {
            taskapp.Run(async context =>{
                await context.Response.WriteAsync("this is task");
            });
            });


           // public delegate Task RequestDelegate(HttpContext context);
           //此时的Func<Task> 为啥说是一个RequestDelegate呢?  applicattion.use内部是如何实现的
        //Func<HttpContext, Func<Task>, Task> middleware 是啥意思?  
        app.Use(async (context, next) =>// 接收一个context和一个Requestdeleagete
            {
                await context.Response.WriteAsync("before start....");
               await  next.Invoke();
            });


            app.Use(next =>//接收一个requestdelegatte 返回一个requestdelegate
            {
                return (context) =>
                {
                    context.Response.WriteAsync("2 in the middleware of start");
                    return next(context);
                };
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("3: start....");
            });
        }
    }
}
