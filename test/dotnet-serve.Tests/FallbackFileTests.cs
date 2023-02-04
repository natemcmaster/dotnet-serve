// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Moq;

namespace McMaster.DotNet.Serve.Tests;

public class FallbackFileTests
{
    private readonly ITestOutputHelper _output;

    public FallbackFileTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ItRespondsWithFallbackFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "FallbackFile");
        using var ds = DotNetServe.Start(path,
            fallbackFile: "fallback-file.html",
            output: _output);
        var resp = await ds.Client.GetWithRetriesAsync("/invalid-path");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task ItRespondsNotFound()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "FallbackFile");
        using var ds = DotNetServe.Start(path,
            fallbackFile: "fallback-file.html",
            output: _output);
        var resp = await ds.Client.GetWithRetriesAsync("/invalid-path.html");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
