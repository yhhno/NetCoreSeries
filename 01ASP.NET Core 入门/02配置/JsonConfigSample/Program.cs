
using System;

using Microsoft.Extensions.Configuration;
namespace JsonConfigSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("class.json",false,true);//参数path为根目录(也就是bin目录下),但新建的接送文件默认是不会生成到debug文件下,所以要修改下class.json文件的属性
            var configuration = builder.Build();

            Console.WriteLine($"ClassNo:{ configuration["ClassNo"]}");
            Console.WriteLine($"ClassDesc:{ configuration["ClassDesc"]}");

            Console.WriteLine("Students");

            Console.Write($"name:{ configuration["Students:0:name"]}");
            Console.WriteLine($"   age:{ configuration["Students:0:age"]}");

            Console.Write($"name:{ configuration["Students:1:name"]}");
            Console.WriteLine($"   age:{ configuration["Students:1:age"]}");

            Console.Write($"name:{ configuration["Students:2:name"]}");
            Console.WriteLine($"   age:{ configuration["Students:2:age"]}");

            Console.ReadLine();
        }
    }
}
