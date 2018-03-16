using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace McMaster.DotNet.Server.DefaultExtensions
{    
    public static class DefaultExtensionsExtensions
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