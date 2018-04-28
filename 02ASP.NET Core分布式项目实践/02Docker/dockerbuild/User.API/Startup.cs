using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;




using User.API.Data;
using Microsoft.EntityFrameworkCore;//对应 UseMySQL

namespace User.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //配置Ef
            services.AddDbContext<UserContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("MysqlUser"));
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            //有bug 有逻辑错误
            InitUserDatabase(app);//可以传递app 
        }

        public void InitUserDatabase(IApplicationBuilder app)
        {
            using (var scope=app.ApplicationServices.CreateScope())//由service container 创建 scope
            {
                var userContext = scope.ServiceProvider.GetRequiredService<UserContext>();//获取实例

                userContext.Database.Migrate();
                if(!userContext.Users.Any())//如果没有用户，就是第一次启动 创建默认用户
                {
                    userContext.Users.Add(new Models.AppUser { Name = "jesse" });
                    userContext.SaveChanges();
                }
            }
        }
    }
}
