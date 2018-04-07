using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
namespace CommandLineSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //4.给configuration一个默认参数
            //初始化一个Dictionary
            var settings = new Dictionary<string, string>
            {
                { "name","默认的Name"},//泛型的Dictionary如何赋值?
                {"age","默认的age" }

            };

            //1.初始化一个ConfigurationBuilder
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)//4.1 将初始化的Dictionary添加到Builde中,如果无参数的话,将使用此默认的
                .AddCommandLine(args);
            //2.获取一个Configuration  键值对
            var configuration = builder.Build();

            //3.输出Configuration的内容
            Console.WriteLine($"name:{configuration["name"]}");
            Console.WriteLine($"age:{configuration["age"]}");

            Console.ReadLine();//阻塞控制台
        }
    }
}
