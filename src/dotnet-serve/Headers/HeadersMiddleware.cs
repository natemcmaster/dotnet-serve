// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace McMaster.DotNet.Serve.Headers
{
    public class HeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HeadersMiddleware> _logger;
        private readonly HeadersOptions _options;

        public HeadersMiddleware(RequestDelegate next, IOptions<HeadersOptions> options, ILogger<HeadersMiddleware> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                foreach (var headerValue in _options.Headers)
                {
                    _logger.LogDebug("Setting header {HeaderName}:{HeaderValue}", headerValue.Key, headerValue.Value);
                    headers[headerValue.Key] = headerValue.Value;
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
