// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.Http.Internal
{
    public class DefaultHttpRequestTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(9001)]
        [InlineData(65535)]
        public void GetContentLength_ReturnsParsedHeader(long value)
        {
            // Arrange
            var request = GetRequestWithContentLength(value.ToString(CultureInfo.InvariantCulture));

            // Act and Assert
            Assert.Equal(value, request.ContentLength);
        }

        [Fact]
        public void GetContentLength_ReturnsNullIfHeaderDoesNotExist()
        {
            // Arrange
            var request = GetRequestWithContentLength(contentLength: null);

            // Act and Assert
            Assert.Null(request.ContentLength);
        }

        [Theory]
        [InlineData("cant-parse-this")]
        [InlineData("-1000")]
        [InlineData("1000.00")]
        [InlineData("100/5")]
        public void GetContentLength_ReturnsNullIfHeaderCannotBeParsed(string contentLength)
        {
            // Arrange
            var request = GetRequestWithContentLength(contentLength);

            // Act and Assert
            Assert.Null(request.ContentLength);
        }

        [Fact]
        public void GetContentType_ReturnsNullIfHeaderDoesNotExist()
        {
            // Arrange
            var request = GetRequestWithContentType(contentType: null);

            // Act and Assert
            Assert.Null(request.ContentType);
        }

        [Fact]
        public void Host_GetsHostFromHeaders()
        {
            // Arrange
            const string expected = "localhost:9001";

            var headers = new HeaderDictionary()
            {
                { "Host", expected },
            };

            var request = CreateRequest(headers);

            // Act
            var host = request.Host;

            // Assert
            Assert.Equal(expected, host.Value);
        }

        [Fact]
        public void Host_DecodesPunyCode()
        {
            // Arrange
            const string expected = "löcalhöst";

            var headers = new HeaderDictionary()
            {
                { "Host", "xn--lcalhst-90ae" },
            };

            var request = CreateRequest(headers);

            // Act
            var host = request.Host;

            // Assert
            Assert.Equal(expected, host.Value);
        }

        [Fact]
        public void Host_EncodesPunyCode()
        {
            // Arrange
            const string expected = "xn--lcalhst-90ae";

            var headers = new HeaderDictionary();

            var request = CreateRequest(headers);

            // Act
            request.Host = new HostString("löcalhöst");

            // Assert
            Assert.Equal(expected, headers["Host"][0]);
        }

        [Fact]
        public void IsHttps_CorrectlyReflectsScheme()
        {
            var request = new DefaultHttpContext().Request;
            Assert.Equal(string.Empty, request.Scheme);
            Assert.False(request.IsHttps);
            request.IsHttps = true;
            Assert.Equal("https", request.Scheme);
            request.IsHttps = false;
            Assert.Equal("http", request.Scheme);
            request.Scheme = "ftp";
            Assert.False(request.IsHttps);
            request.Scheme = "HTTPS";
            Assert.True(request.IsHttps);
        }

        [Fact]
        public void Query_GetAndSet()
        {
            var request = new DefaultHttpContext().Request;
            var requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            Assert.Equal(string.Empty, requestFeature.QueryString);
            Assert.Equal(QueryString.Empty, request.QueryString);
            var query0 = request.Query;
            Assert.NotNull(query0);
            Assert.Equal(0, query0.Count);

            requestFeature.QueryString = "?name0=value0&name1=value1";
            var query1 = request.Query;
            Assert.NotSame(query0, query1);
            Assert.Equal(2, query1.Count);
            Assert.Equal("value0", query1["name0"]);
            Assert.Equal("value1", query1["name1"]);

            var query2 = new QueryCollection( new Dictionary<string, StringValues>()
            {
                { "name2", "value2" }
            });

            request.Query = query2;
            Assert.Same(query2, request.Query);
            Assert.Equal("?name2=value2", requestFeature.QueryString);
            Assert.Equal(new QueryString("?name2=value2"), request.QueryString);
        }

        [Fact]
        public void Cookies_GetAndSet()
        {
            var request = new DefaultHttpContext().Request;
            var cookieHeaders = request.Headers["Cookie"];
            Assert.Empty(cookieHeaders);
            var cookies0 = request.Cookies;
            Assert.Empty(cookies0);
            Assert.Null(cookies0["key0"]);
            Assert.False(cookies0.ContainsKey("key0"));

            var newCookies = new[] { "name0=value0%2C", "%5Ename1=value1" };
            request.Headers["Cookie"] = newCookies;

            cookies0 = RequestCookieCollection.Parse(newCookies);
            var cookies1 = request.Cookies;
            Assert.Equal(cookies0, cookies1);
            Assert.Equal(2, cookies1.Count);
            Assert.Equal("value0,", cookies1["name0"]);
            Assert.Equal("value1", cookies1["^name1"]);
            Assert.Equal(newCookies, request.Headers["Cookie"]);

            var cookies2 = new RequestCookieCollection(new Dictionary<string,string>()
            {
                { "name2", "value2" }
            });
            request.Cookies = cookies2;
            Assert.Equal(cookies2, request.Cookies);
            Assert.Equal("value2", request.Cookies["name2"]);
            cookieHeaders = request.Headers["Cookie"];
            Assert.Equal(new[] { "name2=value2" }, cookieHeaders);
        }

        private static HttpRequest CreateRequest(IHeaderDictionary headers)
        {
            var context = new DefaultHttpContext();
            context.Features.Get<IHttpRequestFeature>().Headers = headers;
            return context.Request;
        }

        private static HttpRequest GetRequestWithContentLength(string contentLength = null)
        {
            return GetRequestWithHeader("Content-Length", contentLength);
        }

        private static HttpRequest GetRequestWithContentType(string contentType = null)
        {
            return GetRequestWithHeader("Content-Type", contentType);
        }

        private static HttpRequest GetRequestWithAcceptHeader(string acceptHeader = null)
        {
            return GetRequestWithHeader("Accept", acceptHeader);
        }

        private static HttpRequest GetRequestWithAcceptCharsetHeader(string acceptCharset = null)
        {
            return GetRequestWithHeader("Accept-Charset", acceptCharset);
        }

        private static HttpRequest GetRequestWithHeader(string headerName, string headerValue)
        {
            var headers = new HeaderDictionary();
            if (headerValue != null)
            {
                headers.Add(headerName, headerValue);
            }

            return CreateRequest(headers);
        }
    }
}
