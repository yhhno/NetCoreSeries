using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcCookieAuthSampleAddUI
{
    /// <summary>
    /// 初始化identity server
    /// </summary>
    public class Config
    {
        //这个会用来所有可以访问的对象，也就是用户的资源
        public static IEnumerable<ApiResource> GetResources()//测试方法 模拟一下 
        {
            return new List<ApiResource>
            {
                new ApiResource("api1","MY Api")
            };
        }

        public static IEnumerable<Client> GetClients()//测试方法 模拟一下 
        {
            //到最后我们会做一个web界面，把它全部统一添加进去，现在其实它是相当于在内存里来管理我们的clients  为啥内存  因为放在list里面
            return new List<Client>//也是会返回客户端的枚举
            {
                new Client()
                {
                    ClientId="mvc",//clientid
                    ClientName="MVC client",
                    ClientUri="http://localhost:5001",
                    LogoUri="https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1524821554&di=5513cf737bfaec6739833409f485d080&imgtype=jpg&er=1&src=http%3A%2F%2Fjbcdn2.b0.upaiyun.com%2F2016%2F05%2Fb69607c7bb75c609307d5218a825cbd9.png",
                    AllowRememberConsent=true,
                

                    AllowedGrantTypes=GrantTypes.Implicit,//允许访问的方式
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())//我们最好对clientsecrect进行简单的加密  Sha256为Identity server的一个扩展方法
                    },

                   //跳转的地址 默认写死 客户端登录地址 它在asp.net core mvc中这个地址是固定的  asp.net core mvc 有个endpooint就是直接默认添加进来就是这个地址，他就会处理认证登录的逻辑 
                   // 这个地方我们只能写在代码里，如果是通过数据库来保存client的话，我们就会直接把这些东西配置到数据库中，正常情况下在生产情况下我们肯定是通过改数据库，肯定不是改代码
                    RedirectUris={"http://localhost:5001/signin-oidc" },//赋值类型为数组  

                    //当它退出的时候，会返回到这个地址
                    PostLogoutRedirectUris={"http://localhost:5001/signout-callback-oidc" },


                    //这个地方就是用户来点那个按钮，你是不是同意我授权,,
                    //好奇如何实现的呢 
                    RequireConsent=true,
            
                    //之前是允许访问的api，现在是用户信息
                    AllowedScopes={
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OpenId,//每一个IdentityResource是一个scope，必须需要的 因为在客户端接受的是 oidc 它会根据oidc 去获取用户信息
                    }// 允许访问的资源
                }
            };
        }
        public static List<TestUser> GetTestUsers()//测试方法 模拟一下  TestUser为 测试的user类
        {
            return new List<TestUser>
            {
               new TestUser
               {
                   SubjectId="10000",//是它id的一个键
                   Username="jesse",
                   Password="123456",
                   Claims={
                       //new Claim ( JwtClaimTypes.Email,"QQ@qq.com"),//JwtClaimTypes.Email 说明啥
                       //new Claim( JwtClaimTypes.Role,"user")
                                              new Claim ("email","QQ@qq.com"),//JwtClaimTypes.Email 说明啥
                       new Claim("role","user")
                   }
               }
            };

        }

        public static  IEnumerable<IdentityResource> GetIdentityResources()//测试方法
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),//这个方法说明了啥？  IdentityResources这是个啥类
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }


    }
}

