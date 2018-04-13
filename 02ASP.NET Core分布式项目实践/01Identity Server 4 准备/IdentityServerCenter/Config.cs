using System;

using System.Linq;
using System.Threading.Tasks;

using System.Collections.Generic;//对应IEnumerable  List
using IdentityServer4.Models;//对应 ApiResource

namespace IdentityServerCenter
{
    /// <summary>
    /// 初始化identity server
    /// </summary>
    public class Config
    {
        //这个会用来所有可以访问的对象，也就是用户的资源
        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api","MY Api")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            //到最后我们会做一个web界面，把它全部统一添加进去，现在其实它是相当于在内存里来管理我们的clients  为啥内存  因为放在list里面
            return new List<Client>//也是会返回客户端的枚举
            {
                new Client()
                {
                    ClientId="client",//clientid
                    AllowedGrantTypes=GrantTypes.ClientCredentials,//允许访问的方式
                    ClientSecrets =
                    {
                        new Secret("secrect".Sha256())//我们最好对clientsecrect进行简单的加密  Sha256为Identity server的一个扩展方法
                    },
                    AllowedScopes={ "api"}// 允许访问的资源
                }
            };
        }
    }
}
