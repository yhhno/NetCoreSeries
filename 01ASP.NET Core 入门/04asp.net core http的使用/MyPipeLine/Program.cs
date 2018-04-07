using System;
using System.Collections.Generic;

using System.Threading.Tasks;
namespace MyPipeLine
{
    class Program
    {
        public static List<Func<RequestDelegate, RequestDelegate>> _list = new List<Func<RequestDelegate, RequestDelegate>>();

        static void Main(string[] args)
        {
            Use(next =>//next 为RequestDelegate
            {
                return context =>//Return为具体的RequestDelegate
                {
                    //Console.WriteLine("1");
                    //return next.Invoke(context);//next为传入的参数, 返回值为Task

                    Console.WriteLine("1");
                    return Task.CompletedTask;  //返回一个Task,但此时阻断了委托链

                };

            });

            Use(next =>
            {
                return context =>
                {
                    Console.WriteLine("2");
                    return next.Invoke(context);
                };

            });

            RequestDelegate end = (context) =>
            {
                Console.WriteLine("end...");
                return Task.CompletedTask;
            };
            _list.Reverse();
            foreach (var middleware in _list)
            {
                end = middleware.Invoke(end);
            }

            end.Invoke(new Context());
            Console.ReadLine();
        }

        public static void Use(Func<RequestDelegate,RequestDelegate> middleware)
        {
            _list.Add(middleware);
        }
    }
}
