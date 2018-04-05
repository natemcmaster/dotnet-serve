// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace McMaster.DotNet.Serve.DefaultExtensions
{
    static class DefaultExtensionsExtensions
    {
        public static IApplicationBuilder UseDefaultExtensions(this IApplicationBuilder app, DefaultExtensionsOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<DefaultExtensionsMiddleware>(Options.Create(options));
        }
    }
}
