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
        private RoleManager<ApplicationUserRole> _roleManager;
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

            if (!context.Roles.Any())
            {
                _roleManager = services.GetRequiredService<RoleManager<ApplicationUserRole>>();//手工依赖注入
                var role = new ApplicationUserRole() { Name = "Adminstrators", NormalizedName = "Adminstrators" };
                var result=await _roleManager.CreateAsync(role);
                if (!result.Succeeded)//如果创建失败，怎么办呢？
                {
                    throw new Exception("初始默认角色失败："+result.Errors.SelectMany(e=>e.Description));
                }
            }



            //我们先查找下是否有用户。 true 如果源序列中不包含任何元素，则否则为 false。
            if (!context.Users.Any())//没有用户的情况下，也就是系统初始化的另一个维度。 此处存在什么问题？
            {
                _userManager = services.GetRequiredService<UserManager<ApplicationUser>>();//手工依赖注入，

                //新建一个user
                var defaultUser = new ApplicationUser
                {
                    UserName = "Administrator",
                    Email = "qq@qq.com",
                    NormalizedUserName = "Admin",
                    Avatar= "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1524821554&di=5513cf737bfaec6739833409f485d080&imgtype=jpg&er=1&src=http%3A%2F%2Fjbcdn2.b0.upaiyun.com%2F2016%2F05%2Fb69607c7bb75c609307d5218a825cbd9.png"
                };

        
               var result= await _userManager.CreateAsync(defaultUser, "123456");//添加角色和 创建用户有没有涉及到事务呢？
                await _userManager.AddToRoleAsync(defaultUser, "Adminstrators"); //添加角色，是不是在数据库创建了用户数据呢？
                if (!result.Succeeded)//如果创建失败，怎么办呢？
                {
                    throw new Exception("初始默认用户失败，");
                }
            }



        }
    }
}
