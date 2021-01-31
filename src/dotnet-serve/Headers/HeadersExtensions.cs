// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace McMaster.DotNet.Serve.Headers
{
    internal static class HeadersExtensions
    {
        public static IApplicationBuilder UseHeaders(this IApplicationBuilder app, HeadersOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<HeadersMiddleware>(Options.Create(options));
        }
    }
}
