// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

internal static class HttpClientExtensions
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

    public static async Task<HttpResponseMessage> SendOptionsWithRetriesAsync(this HttpClient client, string uri, Dictionary<string, List<string>> headers, int retries = 10, ITestOutputHelper output = null)
    {
        while (retries > 0)
        {
            retries--;
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Options, uri);
                foreach (var header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
                return await client.SendAsync(httpRequestMessage);
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

