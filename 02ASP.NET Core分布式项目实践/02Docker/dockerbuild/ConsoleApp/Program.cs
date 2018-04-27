using System;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World From Docker!");
            //Console.ReadLine();


            Console.WriteLine("Hello World From Docker!");

            Thread.Sleep(Timeout.Infinite);//县城一直sleep下去
        }
    }
}
