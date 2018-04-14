using System;


using System.Net.Http; //对应HttpClient
using IdentityModel.Client;

namespace ThirdPartyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //step1 检测验证服务器是否可访问， 可访问返回验证服务器信息
            //返回一个文档，，Client for retrieving OpenID Connect discovery documents 看不明白，就不看了，
            var diso = DiscoveryClient.GetAsync("http://localhost:5000").Result;//因为是异步方法，同步调用的话，调用result属性
            if(diso.IsError)
            {
                Console.WriteLine(diso.Error);
            }

            //step2 根据返回的验证服务器信息， 新建tokenclient 获取token
            var tokenClient = new TokenClient(diso.TokenEndpoint, "client", "secrect");//之前不明白带有Client结尾的类库是干嘛的？
            var tokenResponse = tokenClient.RequestClientCredentialsAsync("api").Result;//同步调用
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }
            else
            {
                Console.WriteLine(tokenResponse.Json);
            }

            //step3, 携带获取的token， 访问api服务器
            var httpClient = new HttpClient();//这里有个啥问题？
            httpClient.SetBearerToken(tokenResponse.AccessToken);
            var response = httpClient.GetAsync("http://localhost:5001/api/values").Result;
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }


            Console.ReadLine();
        }
    }
}
