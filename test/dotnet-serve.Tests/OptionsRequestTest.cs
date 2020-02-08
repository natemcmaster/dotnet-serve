// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    public class OptionsRequestTest
    {
        private readonly ITestOutputHelper _output;

        public OptionsRequestTest(ITestOutputHelper output)
        {
            this._output = output;
        }
        [Fact]
        public async Task ItAllowsOptionsRequestIfCORSIsEnabled()
        {
            using var ds = DotNetServe.Start(enableCors: true, output: _output);
            var request = new HttpRequestMessage(HttpMethod.Options, ds.Client.BaseAddress);
            request.Headers.Add("Origin", "www.google.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            var result = await ds.Client.SendAsync(request);
            //Making sure we are getting 204 and not 404
            Assert.Equal(System.Net.HttpStatusCode.NoContent, result.StatusCode);

            Assert.Contains("*", result.Headers.First(x => x.Key == "Access-Control-Allow-Origin").Value);
            Assert.Contains("GET", result.Headers.First(x => x.Key == "Access-Control-Allow-Methods").Value);
        }

        [Fact]
        public async Task ItDoesNotAllowOptionsRequestIfCORSIsNotEnabled()
        {
            using var ds = DotNetServe.Start();
            var request = new HttpRequestMessage(HttpMethod.Options, ds.Client.BaseAddress);
            request.Headers.Add("Origin", "www.google.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            var result = await ds.Client.SendAsync(request);
            //Making sure we are getting 404 instead of 204 if CORS is not enabled
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
