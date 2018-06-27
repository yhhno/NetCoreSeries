using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


using Microsoft.AspNetCore.Authentication;//对应openidconnect
using Microsoft.IdentityModel.Protocols.OpenIdConnect;//OpenIdConnectResponseType

namespace MVCClient
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
            services.AddMvc();

            //添加Authentication 配置信息
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies"; //这个地方我们用的是cookie方式  这是啥意思？ 本地认证用cookies 也就是是把远程授权后的标识，在客户端的保存方式
                options.DefaultChallengeScheme = "oidc";//当它需要登录的时候我们使用OIDC 也就是我们的openidconnect server  远程授权方式

            })
            .AddCookie()
            .AddOpenIdConnect("oidc", options =>
            {

                //问题 自己看注释，看不出名堂来， 看不明白，也没有意愿去深究，，之记住案例所涉及的的字面意思
                options.SignInScheme = "Cookies";//mvc客户端使用网站这块的登录了
                options.Authority = "http://localhost:5000"; //授权服务器地址
                options.RequireHttpsMetadata = false;//我们也没有证书啥的
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;//有啥意义

                //配置下客户端
                options.ClientId = "mvc";
                options.ClientSecret = "secret";
                options.SaveTokens = true;//是不是保存cookies


                options.GetClaimsFromUserInfoEndpoint = true;//它是发起了另外一个请求，到5000的端口下http://localhost:5000/connent/userinfo 去获取用户的claims
                options.ClaimActions.MapJsonKey("sub", "sub");
                options.ClaimActions.MapJsonKey("preferred_username", "preferred_username");
                options.ClaimActions.MapJsonKey("avatar", "avatar");
                options.ClaimActions.MapCustomJson("role", jobj=>jobj["role"].ToString());

                options.Scope.Add("offline_access");
                options.Scope.Add("opendi");
                options.Scope.Add("profile");

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();


            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
