using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Fakes
{
    public class StartupWithNullConfigureServices
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return null;
        }

        public void Configure(IApplicationBuilder app) { }
    }
}