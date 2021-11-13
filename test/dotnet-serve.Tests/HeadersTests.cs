// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

public class HeadersTests
{
    private readonly ITestOutputHelper _output;

    public HeadersTests(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Fact]
    public async Task ItAppliesHeaders()
    {
        var headers = new Dictionary<string, string>
            {
                {"X-XSS-Protection", "1; mode=block" },
                {"X-Frame-Options", "SAMEORIGIN" },
            };

        var headerArguments = new string[]
        {
                "X-XSS-Protection: 1; mode=block",
                "x-frame-options: SAMEORIGIN",
        };

        var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Mime");
        using var ds = DotNetServe.Start(path, output: _output, headers: headerArguments);
        var resp = await ds.Client.GetWithRetriesAsync("/file.js");

        foreach (var header in headers)
        {
            var respHeader = Assert.Single(resp.Headers, h => h.Key == header.Key);
            var respHeaderValue = Assert.Single(respHeader.Value);
            Assert.Equal(respHeaderValue, header.Value);
        }
    }
}
