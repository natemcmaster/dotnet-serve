// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    public class OptionsRequestTest
    {
        [Fact]
        public async Task ValidateOptionsRequest()
        {
            using var ds = DotNetServe.Start();
            var request = new HttpRequestMessage(HttpMethod.Options, ds.Client.BaseAddress);
            request.Headers.Add("Origin", "www.google.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            var result = await ds.Client.SendAsync(request);
            //Making sure we are getting 204 and not 404
            Assert.Equal(System.Net.HttpStatusCode.NoContent, result.StatusCode);
        }
    }
}
