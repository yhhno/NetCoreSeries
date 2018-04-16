
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Hosting;//对应IWebHost
using Microsoft.EntityFrameworkCore;//对应DbContext
using Microsoft.Extensions.DependencyInjection;//对应host.Services.CreateScope()
using Microsoft.Extensions.Logging;//对应ILogger

namespace MvcCookieAuthSampleAddUI.Data
{
    public static  class WebHostMigrationExtensions
    {
        //我们要扩展的是IWebHost
        //这种Action的用法，在asp.net Core中，应用的非常多，用来做配置的，相当于把方法体内要做的事情，放到外边去做，

        //此方法相当于包装器,  此包装器在哪里使用呢？ 就在program中使用
        public static IWebHost MigrateDbContext<TContext>(this IWebHost host, Action<TContext, IServiceProvider> seed)
            where TContext: DbContext
        {
            //DbContext在做初始化的时候，我们肯定不想和其他DbContext通用，我们想要自己的DbContext
            using (var scope=host.Services.CreateScope())//此区间的实例，只会在此区间有效
            {
                //获取几个实例
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();
                //接下来就来执行。
                try
                {
                    context.Database.Migrate();//相当于 Update-database

                    //执行外部方法，此时提供入参 Action<TContext, IServiceProvider>中 context, services的实例
                    seed(context, services);

                    //执行成功，
                    logger.LogInformation($"执行DbContext {typeof(TContext).Name} 的seed方法成功");
                }
                catch (Exception ex)
                {

                    //报错的话，就记录下
                    logger.LogError(ex, $"执行DbContext {typeof(TContext).Name} 的seed方法失败");
                }
            }

           
            return host;
        }
    }
}
