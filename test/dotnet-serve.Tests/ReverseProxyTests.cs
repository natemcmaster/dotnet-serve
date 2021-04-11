// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

public class ReverseProxyTests
{
    private readonly ITestOutputHelper _output;

    public ReverseProxyTests(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ItAppliesReverseProxies(bool enableProxy)
    {
        var backendPath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "ReverseProxy", "Backend");
        var frontendPath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "ReverseProxy", "Frontend");

        using var backendServer = DotNetServe.Start(backendPath, output: _output);

        string[] proxyMapping = Array.Empty<string>();
        if (enableProxy)
        {
            // Proxy the path /api.json to the backend:
            proxyMapping = new[] { $"/api.json={backendServer.Client.BaseAddress}" };
        }

        using var frontendServer = DotNetServe.Start(frontendPath, output: _output, reverseProxyMap: proxyMapping);

        // Sanity checks:
        Assert.Equal(System.Net.HttpStatusCode.OK, (await frontendServer.Client.GetWithRetriesAsync("/file.html")).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, (await backendServer.Client.GetWithRetriesAsync("/file.html")).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, (await backendServer.Client.GetWithRetriesAsync("/api.json")).StatusCode);

        // Actual proxy test:
        var request = await frontendServer.Client.GetWithRetriesAsync("/api.json");
        if (enableProxy)
        {
            Assert.Equal(System.Net.HttpStatusCode.OK, request.StatusCode);
        }
        else
        {
            Assert.Equal(System.Net.HttpStatusCode.NotFound, request.StatusCode);
        }
    }
}
