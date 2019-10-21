// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    public class BrotliTests
    {
        private readonly ITestOutputHelper _output;

        public BrotliTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public async Task ItCompressesOutput()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Mime");
            using var ds = DotNetServe.Start(path, output: _output, useBrotli: true);
            ds.Client.DefaultRequestHeaders.Add("Accept-Encoding", "br, deflate");
            var resp = await ds.Client.GetWithRetriesAsync("file.js");
            Assert.Equal("br", resp.Content.Headers.ContentEncoding.First());
        }
    }
}
