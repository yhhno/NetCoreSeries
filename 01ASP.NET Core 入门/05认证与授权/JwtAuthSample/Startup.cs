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


using Microsoft.AspNetCore.Authentication.JwtBearer;
using JwtAuthSample.Models;
using Microsoft.IdentityModel.Tokens;//SymmetricSecurityKey 对称加密的一种方式
using System.Text;

namespace JwtAuthSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;//configuration已经存在, 可以来读取配置
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));//给我们,在其他地方使用webconfg的

            //初始化就要使用
            var jwtSettings = new JwtSettings();
            Configuration.Bind("JwtSettings", jwtSettings);

            //配置验证与授权
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//我去验证的时候,你到底有没验证授权的,我需要用到一种方式
            })
            //配置jwtbearer ,参数的设置
            .AddJwtBearer(o =>
            {
                ////   //37
                //o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                //{

                //    //从配置文件中读取出来的
                //    ValidIssuer = jwtSettings.Issuer,
                //    ValidAudience = jwtSettings.Audience,
                //    //引入一个加密的方式
                //    //SymmetricSecurityKey 参数为byte[]类型  
                //    //Encoding.UTF8.GetBytes获取byte[]形式
                //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))

                //}



                //38
                o.SecurityTokenValidators.Clear();//清空验证逻辑
                                                  //添加新的验证逻辑
                o.SecurityTokenValidators.Add(new MyTokenValidator());


               //添加新的token获取方式;
                o.Events = new JwtBearerEvents()//自定义events
                {
                    OnMessageReceived = context =>//替换默认的evnent的OnMessageReceived
                    {
                        var token = context.Request.Headers["mytoken"];
                        context.Token = token.FirstOrDefault();
                        return Task.CompletedTask;
                    }
                };

               
            });



            //39 添加基于Policy授权
            services.AddAuthorization(options =>
            {
            options.AddPolicy("SuperAdminOnly", policy => policy.RequireClaim("SuperAdminOnly"));
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


            app.UseAuthentication();//中间件
            app.UseMvc();
        }
    }
}
