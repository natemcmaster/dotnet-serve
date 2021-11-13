// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

public class OptionsRequestTest
{
    private readonly ITestOutputHelper _output;

    public OptionsRequestTest(ITestOutputHelper output)
    {
        this._output = output;
    }

    private Dictionary<string, List<string>> PrepareCORSHeaders()
    {
        var headers = new Dictionary<string, List<string>>
            {
                { "Origin", new List<string> { "www.google.com" } },
                { "Access-Control-Request-Method", new List<string> { "GET" } }
            };
        return headers;
    }

    [Fact]
    public async Task ItAllowsOptionsRequestIfCORSIsEnabled()
    {
        using var ds = DotNetServe.Start(enableCors: true, output: _output);
        var result = await ds.Client.SendOptionsWithRetriesAsync(ds.Client.BaseAddress.AbsoluteUri, PrepareCORSHeaders(), output: _output);
        //Making sure we are getting 204 and not 404
        Assert.Equal(System.Net.HttpStatusCode.NoContent, result.StatusCode);

        Assert.Contains("*", result.Headers.First(x => x.Key == "Access-Control-Allow-Origin").Value);
        Assert.Contains("GET", result.Headers.First(x => x.Key == "Access-Control-Allow-Methods").Value);
    }

    [Fact]
    public async Task ItDoesNotAllowOptionsRequestIfCORSIsNotEnabled()
    {
        using var ds = DotNetServe.Start();
        var result = await ds.Client.SendOptionsWithRetriesAsync(ds.Client.BaseAddress.AbsoluteUri, PrepareCORSHeaders(), output: _output);
        //Making sure we are getting 404 instead of 204 if CORS is not enabled
        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
    }
}
