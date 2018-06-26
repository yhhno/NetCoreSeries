using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using MvcCookieAuthSampleAddUI.Data;
//using Microsoft.EntityFrameworkCore;//对应UseSqlServer
//using Microsoft.Extensions.Configuration;//对应 IConfiguration
//using MvcCookieAuthSampleAddUI.Models;
//using Microsoft.AspNetCore.Identity;//对应AddDefaultTokenProviders



using IdentityServer4;//对应services.AddIdentityServer()
using Microsoft.Extensions.Configuration;
using MvcCookieAuthSampleAddUI.Services;

namespace MvcCookieAuthSampleAddUI
{
    public class Startup
    {
        public IConfiguration Configuration;
        public Startup(IConfiguration configuration)//获取配置操作
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //在添加mvc之前 添加 Identitysever
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()//我们要一个开发的证书

                //更改Identity server 4配置 
                .AddInMemoryApiResources(Config.GetResources())
                .AddInMemoryClients(Config.GetClients())
                //添加GetTestUsers到配置中区
                .AddTestUsers(Config.GetTestUsers())

                //添加identityResource
                .AddInMemoryIdentityResources(Config.GetIdentityResources());



            #region old
            ////配置EF
            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));//EF使用SqlServer,并获取连接字符串,也就是获取配置信息
            //});

            ////配置Identity
            //services.AddIdentity<ApplicationUser, ApplicationUserRole>()//为啥是这两个参数
            //    .AddEntityFrameworkStores<ApplicationDbContext>()//为啥有这步骤
            //    .AddDefaultTokenProviders();//这个有啥用
            ////Identity默认情况下,对密码格式限制非常严格,我们修改下,应该是配置,那肯定是修改option了
            //services.Configure<IdentityOptions>(options => {
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequireNonAlphanumeric = true;
            //    options.Password.RequiredLength = 12;
            //});


            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        options.LoginPath = "/Account/Login";
            //    });//配置
            #endregion

            services.AddScoped<ConsentService>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
