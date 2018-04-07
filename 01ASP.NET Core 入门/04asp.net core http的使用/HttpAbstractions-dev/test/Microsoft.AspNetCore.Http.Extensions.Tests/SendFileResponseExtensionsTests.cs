// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Microsoft.AspNetCore.Http.Extensions.Tests
{
    public class SendFileResponseExtensionsTests
    {
        [Fact]
        public Task SendFileWhenFileNotFoundThrows()
        {
            var response = new DefaultHttpContext().Response;
            return Assert.ThrowsAsync<FileNotFoundException>(() => response.SendFileAsync("foo"));
        }

        [Fact]
        public async Task SendFileWorks()
        {
            var context = new DefaultHttpContext();
            var response = context.Response;
            var fakeFeature = new FakeSendFileFeature();
            context.Features.Set<IHttpSendFileFeature>(fakeFeature);

            await response.SendFileAsync("bob", 1, 3, CancellationToken.None);

            Assert.Equal("bob", fakeFeature.name);
            Assert.Equal(1, fakeFeature.offset);
            Assert.Equal(3, fakeFeature.length);
            Assert.Equal(CancellationToken.None, fakeFeature.token);
        }

        private class FakeSendFileFeature : IHttpSendFileFeature
        {
            public string name = null;
            public long offset = 0;
            public long? length = null;
            public CancellationToken token;

            public Task SendFileAsync(string path, long offset, long? length, CancellationToken cancellation)
            {
                this.name = path;
                this.offset = offset;
                this.length = length;
                this.token = cancellation;
                return Task.FromResult(0);
            }
        }
    }
}
