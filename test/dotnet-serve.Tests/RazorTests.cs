// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    public class RazorTests
    {
        private readonly ITestOutputHelper _output;

        public RazorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ItCanLoadSimpleRazorPage()
        {
            using (var serve = DotNetServe.Start(
                    directory: Path.Combine(AppContext.BaseDirectory, "TestAssets", "RazorSite"),
                    output: _output,
                    enableRazor: true))
            {
                var result = await serve.Client.GetStringAsync("/");
                Assert.Contains("<li>Item 2</li>", result);
            }
        }
    }
}
