// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Builder.Extensions
{
    public class MapPathMiddlewareTests
    {
        private static readonly Action<IApplicationBuilder> ActionNotImplemented = new Action<IApplicationBuilder>(_ => { throw new NotImplementedException(); });

        private static Task Success(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Items["test.PathBase"] = context.Request.PathBase.Value;
            context.Items["test.Path"] = context.Request.Path.Value;
            return Task.FromResult<object>(null);
        }

        private static void UseSuccess(IApplicationBuilder app)
        {
            app.Run(Success);
        }

        private static Task NotImplemented(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static void UseNotImplemented(IApplicationBuilder app)
        {
            app.Run(NotImplemented);
        }

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new ApplicationBuilder(serviceProvider: null);
            var noMiddleware = new ApplicationBuilder(serviceProvider: null).Build();
            var noOptions = new MapOptions();
            Assert.Throws<ArgumentNullException>(() => builder.Map("/foo", configuration: null));
            Assert.Throws<ArgumentNullException>(() => new MapMiddleware(noMiddleware, null));
        }

        [Theory]
        [InlineData("/foo", "", "/foo")]
        [InlineData("/foo", "", "/foo/")]
        [InlineData("/foo", "/Bar", "/foo")]
        [InlineData("/foo", "/Bar", "/foo/cho")]
        [InlineData("/foo", "/Bar", "/foo/cho/")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho/do")]
        public void PathMatchFunc_BranchTaken(string matchPath, string basePath, string requestPath)
        {
            HttpContext context = CreateRequest(basePath, requestPath);
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.Map(matchPath, UseSuccess);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
        }

        [Theory]
        [InlineData("/foo", "", "/foo")]
        [InlineData("/foo", "", "/foo/")]
        [InlineData("/foo", "/Bar", "/foo")]
        [InlineData("/foo", "/Bar", "/foo/cho")]
        [InlineData("/foo", "/Bar", "/foo/cho/")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho/do")]
        [InlineData("/foo", "", "/Foo")]
        [InlineData("/foo", "", "/Foo/")]
        [InlineData("/foo", "/Bar", "/Foo")]
        [InlineData("/foo", "/Bar", "/Foo/Cho")]
        [InlineData("/foo", "/Bar", "/Foo/Cho/")]
        [InlineData("/foo/cho", "/Bar", "/Foo/Cho")]
        [InlineData("/foo/cho", "/Bar", "/Foo/Cho/do")]
        public void PathMatchAction_BranchTaken(string matchPath, string basePath, string requestPath)
        {
            HttpContext context = CreateRequest(basePath, requestPath);
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.Map(matchPath, subBuilder => subBuilder.Run(Success));
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath + requestPath.Substring(0, matchPath.Length), (string)context.Items["test.PathBase"]);
            Assert.Equal(requestPath.Substring(matchPath.Length), context.Items["test.Path"]);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/foo/")]
        [InlineData("/foo/cho/")]
        public void MatchPathWithTrailingSlashThrowsException(string matchPath)
        {
            Assert.Throws<ArgumentException>(() => new ApplicationBuilder(serviceProvider: null).Map(matchPath, map => { }).Build());
        }

        [Theory]
        [InlineData("/foo", "", "")]
        [InlineData("/foo", "/bar", "")]
        [InlineData("/foo", "", "/bar")]
        [InlineData("/foo", "/foo", "")]
        [InlineData("/foo", "/foo", "/bar")]
        [InlineData("/foo", "", "/bar/foo")]
        [InlineData("/foo/bar", "/foo", "/bar")]
        public void PathMismatchFunc_PassedThrough(string matchPath, string basePath, string requestPath)
        {
            HttpContext context = CreateRequest(basePath, requestPath);
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.Map(matchPath, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
        }

        [Theory]
        [InlineData("/foo", "", "")]
        [InlineData("/foo", "/bar", "")]
        [InlineData("/foo", "", "/bar")]
        [InlineData("/foo", "/foo", "")]
        [InlineData("/foo", "/foo", "/bar")]
        [InlineData("/foo", "", "/bar/foo")]
        [InlineData("/foo/bar", "/foo", "/bar")]
        public void PathMismatchAction_PassedThrough(string matchPath, string basePath, string requestPath)
        {
            HttpContext context = CreateRequest(basePath, requestPath);
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.Map(matchPath, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
        }

        [Fact]
        public void ChainedRoutes_Success()
        {
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.Map("/route1", map =>
            {
                map.Map("/subroute1", UseSuccess);
                map.Run(NotImplemented);
            });
            builder.Map("/route2/subroute2", UseSuccess);
            var app = builder.Build();

            HttpContext context = CreateRequest(string.Empty, "/route1");
            Assert.Throws<AggregateException>(() => app.Invoke(context).Wait());

            context = CreateRequest(string.Empty, "/route1/subroute1");
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route1/subroute1", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2");
            app.Invoke(context).Wait();
            Assert.Equal(404, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2/subroute2");
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2/subroute2", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2/subroute2/subsub2");
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2/subroute2/subsub2", context.Request.Path.Value);
        }

        private HttpContext CreateRequest(string basePath, string requestPath)
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.PathBase = new PathString(basePath);
            context.Request.Path = new PathString(requestPath);
            return context;
        }
    }
}
