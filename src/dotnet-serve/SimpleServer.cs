// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using McMaster.DotNet.Server.RazorPages;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Server
{
    class SimpleServer
    {
        private readonly CommandLineOptions _options;
        private readonly IConsole _console;

        private static readonly IPAddress[] s_defaultAddresses = {
            IPAddress.Loopback,
            IPAddress.Any,
            IPAddress.IPv6Any,
        };

        public SimpleServer(CommandLineOptions options, IConsole console)
        {
            _options = options;
            _console = console;
        }

        public async Task<int> RunAsync()
        {
            var cts = new CancellationTokenSource();
            var addresses = _options.Addresses.Length > 0 ? _options.Addresses : s_defaultAddresses;
            var directory = _options.Directory;
            var port = _options.Port;

            _console.CancelKeyPress += (o, e) =>
            {
                _console.WriteLine("Shutting down...");
                cts.Cancel();
            };

            var path = directory != null
                ? Path.GetFullPath(directory)
                : Directory.GetCurrentDirectory();                  

            var host = new WebHostBuilder()
                .ConfigureLogging(l =>
                {
                    l.SetMinimumLevel(LogLevel.Information);
                    l.AddConsole();
                })
                .PreferHostingUrls(false)
                .UseKestrel(o =>
                {
                    foreach (var a in addresses)
                    {
                        if (a.Equals(IPAddress.Loopback) || a.Equals(IPAddress.IPv6Loopback))
                        {
                            o.ListenLocalhost(port);
                        }
                        else
                        {
                            o.Listen(a, port);
                        }
                    }
                })
                .UseSockets()
                .UseWebRoot(path)
                .UseContentRoot(path)
                .UseEnvironment("Production")
                .SuppressStatusMessages(true)
                .UseStartup<Startup>()
                .ConfigureServices(s =>
                    s.AddSingleton(_options)
                    .AddSingleton<RazorPageSourceProvider>())
                .Build();

            _console.ForegroundColor = ConsoleColor.DarkYellow;
            _console.Write("Starting server, serving ");
            _console.ResetColor();
            _console.WriteLine(Path.GetRelativePath(Directory.GetCurrentDirectory(), path));

            string[] defaultExtensions = _options.GetDefaultExtensions();          
            if(defaultExtensions != null)
            {                
                _console.ForegroundColor = ConsoleColor.DarkYellow;
                _console.WriteLine($"Using default extensions " + string.Join(", ", defaultExtensions));
                _console.ResetColor();
            }

            await host.StartAsync(cts.Token);
            AfterServerStart(host);
            await host.WaitForShutdownAsync(cts.Token);
            return 0;
        }

        private void AfterServerStart(IWebHost host)
        {
            var addresses = host.ServerFeatures.Get<IServerAddressesFeature>();
            var pathBase = _options.GetPathBase();

            _console.WriteLine("Listening on:");
            _console.ForegroundColor = ConsoleColor.Green;
            foreach (var a in addresses.Addresses)
            {
                _console.WriteLine("  " + a + pathBase);
            }

            _console.ResetColor();
            _console.WriteLine("");
            _console.WriteLine("Press CTRL+C to exit");

            if (_options.OpenBrowser)
            {
                var url = addresses.Addresses.First();
                // normalize to loopback if binding to IPAny
                url = url.Replace("0.0.0.0", "localhost");
                url = url.Replace("[::]", "localhost");

                if (!string.IsNullOrEmpty(pathBase))
                {
                    url += pathBase;
                }

                LaunchBrowser(url);
            }
        }

        private static void LaunchBrowser(string url)
        {
            string processName;
            string[] args;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                processName = "open";
                args = new[] { url };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                processName = "xdg-open";
                args = new[] { url };
            }
            else
            {
                processName = "cmd";
                args = new[] { "/C", "start", url };
            }

            Process.Start(processName, ArgumentEscaper.EscapeAndConcatenate(args));
        }
    }
}
