// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    static class HttpClientExtensions
    {
        public static async Task<string> GetStringWithRetriesAsync(this HttpClient client, string uri, int retries = 10, ITestOutputHelper output = null)
        {
            while (retries > 0)
            {
                retries--;
                try
                {
                    return await client.GetStringAsync(uri);
                }
                catch (Exception ex)
                {
                    output?.WriteLine($"Request to {uri} failed with '{ex.Message}'");
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                }
            }

            throw new TimeoutException("Failed to connect to " + uri);
        }

        public static async Task<HttpResponseMessage> GetWithRetriesAsync(this HttpClient client, string uri, int retries = 10, ITestOutputHelper output = null)
        {
            while (retries > 0)
            {
                retries--;
                try
                {
                    return await client.GetAsync(uri);
                }
                catch (Exception ex)
                {
                    output?.WriteLine($"Request to {uri} failed with '{ex.Message}'");
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                }
            }

            throw new TimeoutException("Failed to connect to " + uri);
        }
    }
}

