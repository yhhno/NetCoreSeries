using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var query = new QueryBuilder()
            {
                { "hello", "world" }
            }.ToQueryString();

            var uri = UriHelper.BuildAbsolute("http", new HostString("contoso.com"), query: query);

            Console.WriteLine(uri);
        }
    }
}
