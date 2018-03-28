// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using McMaster.DotNet.Server.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace McMaster.DotNet.Server
{
    class Startup
    {
        private readonly CommandLineOptions _options;
        private readonly RazorPageSourceProvider _sourceProvider;

        public Startup(
            CommandLineOptions options,
            RazorPageSourceProvider sourceProvider)
        {
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
                      });
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

            if (!string.IsNullOrEmpty(_options.PathBase))
            {
                app.Map(_options.PathBase, ConfigureFileServer);
            }
            else
            {
                ConfigureFileServer(app);
            }
        }

        private void ConfigureFileServer(IApplicationBuilder app)
        {
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
