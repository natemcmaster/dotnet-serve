// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using McMaster.DotNet.Serve.DefaultExtensions;
using McMaster.DotNet.Serve.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace McMaster.DotNet.Serve
{
    class Startup
    {
        private readonly IHostingEnvironment _environment;
        private readonly CommandLineOptions _options;
        private readonly RazorPageSourceProvider _sourceProvider;

        public Startup(
            IHostingEnvironment environment,
            CommandLineOptions options,
            RazorPageSourceProvider sourceProvider)
        {
            _environment = environment;
            _options = options;
            _sourceProvider = sourceProvider;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<KeyManagementOptions>(o => o.XmlRepository = new EphemeralXmlRepository())
                .AddSingleton<IKeyManager, KeyManager>()
                .AddSingleton<IAuthorizationPolicyProvider, NullAuthPolicyProvider>();

            if (_options.EnableRazor)
            {
                Debug.Assert(_sourceProvider != null, "Source provider should be configured");
                services.AddMvcCore()
                      .AddRazorPages()
                      .WithRazorPagesRoot("/")
                      .AddRazorViewEngine(o =>
                      {
                          o.CompilationCallback = _sourceProvider.OnCompilation;
                      })
                      .PartManager.ApplicationParts.Add(new TpaReferencesProvider());
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusCodePages("text/html",
                       "<html><head><title>Error {0}</title></head><body><h1>HTTP {0}</h1></body></html>");

            app.UseDeveloperExceptionPage();
            if (_options.EnableRazor)
            {
                app.UseMvcWithDefaultRoute();
                _sourceProvider.Initialize();
            }

            var pathBase = _options.GetPathBase();
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.Map(pathBase, ConfigureFileServer);
            }
            else
            {
                ConfigureFileServer(app);
            }
        }

        private void ConfigureFileServer(IApplicationBuilder app)
        {
            if (_options.ExcludedFiles.Count > 0)
            {
                var excludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var exclusion in _options.ExcludedFiles)
                {
                    var path = Path.GetRelativePath(_environment.WebRootPath, exclusion);
                    excludes.Add("/" + path);
                }

                app.Use(async (ctx, next) =>
                {
                    if (excludes.Contains(ctx.Request.Path))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    await next();
                });
            }

            var defaultExtensions = _options.GetDefaultExtensions();
            if (defaultExtensions != null && defaultExtensions.Length > 0)
            {
                app.UseDefaultExtensions(new DefaultExtensionsOptions
                {
                    Extensions = defaultExtensions
                });
            }

            app.UseFileServer(new FileServerOptions
            {
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    ServeUnknownFileTypes = true,
                },
            });
        }
    }
}
