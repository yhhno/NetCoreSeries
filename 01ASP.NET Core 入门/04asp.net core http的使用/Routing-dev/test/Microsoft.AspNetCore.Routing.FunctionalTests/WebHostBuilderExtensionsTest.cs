﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Routing.FunctionalTests
{
    public class WebHostBuilderExtensionsTest
    {
        public static TheoryData<Action<IRouteBuilder>, HttpRequestMessage, string> MatchesRequest
        {
            get
            {
                return new TheoryData<Action<IRouteBuilder>, HttpRequestMessage, string>()
                {
                    {
                        (rb) => rb.MapGet("greeting/{name}", (req, resp, routeData) => resp.WriteAsync($"Hello! {routeData.Values["name"]}")),
                        new HttpRequestMessage(HttpMethod.Get, "greeting/James"),
                        "Hello! James"
                    },
                    {
                        (rb) => rb.MapPost(
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new HttpRequestMessage(HttpMethod.Post, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                    {
                        (rb) => rb.MapPut(
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new HttpRequestMessage(HttpMethod.Put, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                    {
                        (rb) => rb.MapDelete("greeting/{name}", (req, resp, routeData) => resp.WriteAsync($"Hello! {routeData.Values["name"]}")),
                        new HttpRequestMessage(HttpMethod.Delete, "greeting/James"),
                        "Hello! James"
                    },
                    {
                        (rb) => rb.MapVerb(
                            "POST",
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new HttpRequestMessage(HttpMethod.Post, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MatchesRequest))]
        public async Task UseRouter_MapGet_MatchesRequest(Action<IRouteBuilder> routeBuilder, HttpRequestMessage request, string expected)
        {
            // Arrange
            var webhostbuilder = new WebHostBuilder();
            webhostbuilder
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseRouter(routeBuilder);
                });
            var testServer = new TestServer(webhostbuilder);
            var client = testServer.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, actual);
        }
    }
}
