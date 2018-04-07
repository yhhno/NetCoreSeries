using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Identity;//对应UserManager
using MvcCookieAuthSampleAddUI.Models;//对应ApplicationUser
using Microsoft.Extensions.DependencyInjection;//依赖注入的命名空间， 对应services.CreateScope()

namespace MvcCookieAuthSampleAddUI.Data
{
    public class ApplicationDbContextSeed
    {
        //创建用户，肯定要用到UserManager
        private UserManager<ApplicationUser> _userManager;
        public async Task SeedAsync(ApplicationDbContext  context, IServiceProvider  services
            )//IServiceProvider是依赖注入的容器， 我们拿到它。
        {
            ////这个地方我们最好Create一个Scope，因为我们不希望，我们的ApplicationDbContext和其他地方 用同一个，也就是共用。
            ////依赖注入的命名空间添加进来
            //using (var scope=services.CreateScope())
            //{
            //    //获取一个userManager实例，通过依赖注入容器。,不保存在外部了。
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //    //我们先查找下是否有用户。
            //}

            //我们先查找下是否有用户。 true 如果源序列中不包含任何元素，则否则为 false。
            if (!context.Users.Any())//没有用户的情况下，也就是系统初始化的另一个维度。 此处存在什么问题？
            {
                _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                //新建一个user
                var defaultUser = new ApplicationUser
                {
                    UserName = "Administrator",
                    Email = "qq@qq.com",
                    NormalizedUserName = "Admin"
                };

                
               var result= await _userManager.CreateAsync(defaultUser, "password");
                if (!result.Succeeded)//如果创建失败，怎么办呢？
                {
                    throw new Exception("初始默认用户失败，");
                }
            }



        }
    }
}
