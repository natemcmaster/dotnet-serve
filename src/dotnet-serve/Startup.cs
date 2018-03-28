using McMaster.DotNet.Server.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace McMaster.DotNet.Server
{
    class Startup
    {
        public Startup(
            IConfiguration config,
            RazorPageSourceProvider sourceProvider)
        {
            Config = config;
            _sourceProvider = sourceProvider;
        }

        IConfiguration Config { get; }

        private readonly RazorPageSourceProvider _sourceProvider;

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                    .AddRazorPages()
                    .WithRazorPagesRoot("/")
                    .AddRazorViewEngine(o =>
                    {
                        o.CompilationCallback = _sourceProvider.OnCompilation;
                    })
                    .Services
                .Configure<KeyManagementOptions>(o => o.XmlRepository = new EphemeralXmlRepository())
                .AddSingleton<IKeyManager, KeyManager>()
                .AddSingleton<IAuthorizationPolicyProvider, NullAuthPolicyProvider>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusCodePages("text/html",
                       "<html><head><title>Error {0}</title></head><body><h1>HTTP {0}</h1></body></html>");

            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            if (!string.IsNullOrEmpty(Config["DotNetServe:PathBase"]))
            {
                app.Map(Config["DotNetServe:PathBase"], ConfigureFileServer);
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
