using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    AllowedGrantTypes=GrantTypes.Implicit,//允许访问的方式
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())//我们最好对clientsecrect进行简单的加密  Sha256为Identity server的一个扩展方法
                    },
                    AllowedScopes={ "api1"}// 允许访问的资源
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
                   Password="123456"
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

