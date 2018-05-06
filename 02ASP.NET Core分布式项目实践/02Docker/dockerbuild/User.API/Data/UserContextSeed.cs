
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;//IApplicationBuilder
using Microsoft.EntityFrameworkCore;//Migrate
using Microsoft.Extensions.DependencyInjection;//CreateScope
using Microsoft.Extensions.Logging;//ILogger
namespace User.API.Data
{
    public class UserContextSeed
    {
        private ILogger<UserContextSeed> _logger;

        public UserContextSeed(ILogger<UserContextSeed> logger)
        {
            _logger = logger;
        }

        public static async Task SeedAsync(IApplicationBuilder applicationBuilder,ILoggerFactory loggerFactory,int? retry=0)
        {
            //添加重试机制
            var retryForAvaiability = retry.Value;
            //应该放到try{ }的范围内，在catch之后会再一次调用SeedAsync方法生成新的scope，前面的scope应该被释放掉。
            try
            {
                using (var scope = applicationBuilder.ApplicationServices.CreateScope())
                {
                    var context = (UserContext)scope.ServiceProvider.GetService(typeof(UserContext));
                    var logger = (ILogger<UserContextSeed>)scope.ServiceProvider.GetServices(typeof(ILogger<UserContextSeed>));
                    logger.LogDebug("Begin UserContextSeed SeedAsync");
                    context.Database.Migrate();
                    if (!context.Users.Any())
                    {
                        context.Users.Add(new Models.AppUser { Name = "jesse" });
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
               
                if (retryForAvaiability<10)
                {
                    retryForAvaiability++;
                    var logger = loggerFactory.CreateLogger(typeof(UserContextSeed));
                    logger.LogError(ex.Message);

                    await SeedAsync(applicationBuilder, loggerFactory, retryForAvaiability);//重试
                }
               
            }

           
        }

    }
}
