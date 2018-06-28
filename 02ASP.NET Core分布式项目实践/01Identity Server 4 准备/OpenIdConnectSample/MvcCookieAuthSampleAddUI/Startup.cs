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
using MvcCookieAuthSampleAddUI.Data;
using MvcCookieAuthSampleAddUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.Services; //IProfileService

using IdentityServer4.EntityFramework;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;//PersistedGrantDbContext
using IdentityServer4.EntityFramework.Mappers;//apiResource.ToEntity()

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
            const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer4.Quitstart;Trusted_Connection=True;MultipleActiveResultSets=true";
            var migartionAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            //配置EF
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));//EF使用SqlServer,并获取连接字符串,也就是获取配置信息
            });

            //配置Identity
            services.AddIdentity<ApplicationUser, ApplicationUserRole>()//为啥是这两个参数
                .AddEntityFrameworkStores<ApplicationDbContext>()//为啥有这步骤
                .AddDefaultTokenProviders();//这个有啥用


             //Identity默认情况下,对密码格式限制非常严格,我们修改下,应该是配置,那肯定是修改option了
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 2;
            });

          
        

            //在添加mvc之前 添加 Identitysever
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()//我们要一个开发的证书

            #region old
                ////更改Identity server 4配置 
                //.AddInMemoryApiResources(Config.GetResources())
                //.AddInMemoryClients(Config.GetClients())
                ////添加identityResource
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())

                ////添加GetTestUsers到配置中区  测试的
                //.AddTestUsers(Config.GetTestUsers());
            #endregion

                //配置configration的ef context 初始化数据库
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migartionAssembly));//sql.MigrationsAssembly() 就是当有新的migration，就执行更新
                    };
                })


                //Operation context 初始化数据库
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migartionAssembly));//sql.MigrationsAssembly() 就是当有新的migration，就执行更新
                    };
                })




                //真实的数据库
                .AddAspNetIdentity<ApplicationUser>()
                .Services.AddTransient<IProfileService, ProfileService>();//添加依赖注入
            



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

            services.AddScoped<ConsentService>();//自定义的依赖注入
            //services.AddScoped<ProfileService>();//非自定义的依赖注入 这样写就不对了额
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


            InitIdentityServerDataBase(app);
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }


        public void InitIdentityServerDataBase(IApplicationBuilder app)
        {
            using (var scope= app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();//执行PersistedGrantDbContext的migrate方法，因为不需要用PersistedGrantDbContext的实例来操作，所以没有赋值给一个变量
                var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();//获取ConfigurationDbContext的实例，来进行数据库操作，也就是添加client等

                //下面的代码，说明client和apiresoource和identityresources等都全部走数据库，非内存
                if(!configurationDbContext.Clients.Any())
                {
                    foreach (var client in Config.GetClients())//数据源为内存中的client集合
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());//需要把client类型转化为IdentityServer4.EntityFramework.Entities.Client
                    }
                    configurationDbContext.SaveChanges();
                }
                if (!configurationDbContext.ApiResources.Any())
                {
                    foreach (var apiResource in Config.GetResources())//数据源为内存中的apiresource集合
                    {
                        configurationDbContext.ApiResources.Add(apiResource.ToEntity());//需要把apiresource类型转化为IdentityServer4.EntityFramework.Entities.ApiResource
                    }
                    configurationDbContext.SaveChanges();
                }
                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var identityResource in Config.GetIdentityResources())//数据源为内存中的IdentityResource集合
                    {
                        configurationDbContext.IdentityResources.Add(identityResource.ToEntity());//需要把client类型转化为IdentityServer4.EntityFramework.Entities.IdentityResource
                    }
                    configurationDbContext.SaveChanges();
                }

            }
        }
    }
}
