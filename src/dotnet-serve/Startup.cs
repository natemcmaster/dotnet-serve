// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using McMaster.DotNet.Serve.DefaultExtensions;
using McMaster.DotNet.Serve.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Forwarder;

namespace McMaster.DotNet.Serve;

internal class Startup
{
    private readonly IWebHostEnvironment _environment;
    private readonly CommandLineOptions _options;

    public Startup(
        IWebHostEnvironment environment,
        CommandLineOptions options)
    {
        _environment = environment;
        _options = options;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .Configure<KeyManagementOptions>(o => o.XmlRepository = new EphemeralXmlRepository())
            .AddSingleton<IKeyManager, KeyManager>()
            .AddSingleton<IAuthorizationPolicyProvider, NullAuthPolicyProvider>();

        if (_options.EnableCors == true)
        {
            services.AddCors();
        }

        services.AddResponseCompression(options =>
        {
            options.Providers.Clear();

            if (_options.UseGzip == true || _options.UseBrotli == true)
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes
                    .Append("application/dll")
                    .Append("application/dat");
            }

            if (_options.UseGzip == true)
            {
                options.Providers.Add<GzipCompressionProvider>();
            }


            if (_options.UseBrotli == true)
            {
                options.Providers.Add<BrotliCompressionProvider>();
            }
        });

        // Reverse proxy:
        services.AddRouting();
        services.AddHttpForwarder();
    }

    public void Configure(IApplicationBuilder app, IHttpForwarder forwarder)
    {
        if (_options.EnableCors == true)
        {
            app.UseCors(corsPolicy =>
            {
                corsPolicy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        }
        app.UseStatusCodePages("text/html",
                   "<html><head><title>Error {0}</title></head><body><h1>HTTP {0}</h1></body></html>");

        app.UseDeveloperExceptionPage();

        ConfigureReverseProxy(app, forwarder);

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

    private void ConfigureReverseProxy(IApplicationBuilder app, IHttpForwarder forwarder)
    {
        var mappings = _options.GetReverseProxyMappings();
        if (mappings?.Any() == true)
        {
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                foreach (var mapping in mappings)
                {
                    endpoints.Map(mapping.Key, async httpContext =>
                    {
                        await forwarder.SendAsync(httpContext, mapping.Value, httpClient, requestOptions);
                    });
                }
            });
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
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
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

        var contentTypeProvider = new FileExtensionContentTypeProvider();

        contentTypeProvider.Mappings.Add(".dll", "application/dll");
        contentTypeProvider.Mappings.Add(".dat", "application/dat");

        var mimeMappings = _options.GetMimeMappings();
        if (mimeMappings != null)
        {
            foreach (var mapping in mimeMappings)
            {
                if (mapping.Value == null)
                {
                    contentTypeProvider.Mappings.Remove(mapping.Key);
                }
                else
                {
                    contentTypeProvider.Mappings[mapping.Key] = mapping.Value;
                }
            }
        }

        var headers = _options.GetHeaders();
        if (headers != null && headers.Count > 0)
        {
            app.UseHeaders(new HeadersOptions
            {
                Headers = headers
            });
        }

        if (_options.UseGzip == true || _options.UseBrotli == true)
        {
            app.UseResponseCompression();
        }

        app.UseFileServer(new FileServerOptions
        {
            EnableDefaultFiles = true,
            EnableDirectoryBrowsing = true,
            StaticFileOptions =
                {
                    ServeUnknownFileTypes = true,
                    ContentTypeProvider = contentTypeProvider
                },
        });

        if (!string.IsNullOrEmpty(_options.FallbackFile))
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToFile(_options.FallbackFile);
            });
        }
    }
}
