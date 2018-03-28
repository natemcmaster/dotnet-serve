// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using McMaster.DotNet.Server.RazorPages;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IOPath = System.IO.Path;

namespace McMaster.DotNet.Server
{
    [Command(
        Name = "dotnet serve",
        FullName = "dotnet-serve",
        Description = "Provides a simple HTTP server")]
    [HelpOption]
    class SimpleServer
    {
        [Argument(0, Name = "path", Description = "Base path to the server root")]
        [DirectoryExists]
        public string Path { get; }

        [Option(Description = "Port to use [8080]. Use 0 for a dynamic port.")]
        [Range(0, 65535, ErrorMessage = "Invalid port. Ports must be in the range of 0 to 65535.")]
        public int Port { get; } = 8080;

        [Option(Description = "Address to use [0.0.0.0]")]
        [IPAddress]
        public string[] Address { get; }

        [Option(Description = "Open a web browser when the server starts. [false]")]
        public bool OpenBrowser { get; }

        [Option("--path-base <PATH>", Description = "The base URL path of postpended to the site url.")]
        public string PathBase { get; private set; }

        public async Task<int> OnExecute(IConsole console)
        {
            var cts = new CancellationTokenSource();
            console.CancelKeyPress += (o, e) =>
            {
                console.WriteLine("Shutting down...");
                cts.Cancel();
            };

            IPAddress[] addresses;
            if (Address.Length == 0)
            {
                addresses = new IPAddress[]
                {
                    IPAddress.Loopback,
                    IPAddress.Any,
                    IPAddress.IPv6Any,
                };
            }
            else
            {
                addresses = new IPAddress[Address.Length];
                for (int i = 0; i < Address.Length; i++)
                {
                    addresses[i] = IPAddress.Parse(Address[i]);
                }
            }

            var path = Path != null
                ? IOPath.GetFullPath(Path)
                : Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(PathBase) && PathBase[0] != '/')
            {
                PathBase = "/" + PathBase;
            }

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
                        if (a.Equals(IPAddress.Loopback))
                        {
                            o.ListenLocalhost(Port);
                        }
                        else
                        {
                            o.Listen(a, Port);
                        }
                    }
                })
                .UseSockets()
                .UseWebRoot(path)
                .UseContentRoot(path)
                .UseEnvironment("Production")
                .SuppressStatusMessages(true)
                .UseSetting("DotNetServe:PathBase", PathBase)
                .UseStartup<Startup>()
                .ConfigureServices(s => s
                    .AddSingleton<RazorPageSourceProvider>())
                .Build();

            console.ForegroundColor = ConsoleColor.DarkYellow;
            console.Write("Starting server, serving ");
            console.ResetColor();
            console.WriteLine(IOPath.GetRelativePath(Directory.GetCurrentDirectory(), path));

            await host.StartAsync(cts.Token);
            AfterServerStart(console, host);
            await host.WaitForShutdownAsync(cts.Token);
            return 0;
        }

        private void AfterServerStart(IConsole console, IWebHost host)
        {
            var addresses = host.ServerFeatures.Get<IServerAddressesFeature>();

            console.WriteLine("Listening on:");
            console.ForegroundColor = ConsoleColor.Green;
            foreach (var a in addresses.Addresses)
            {
                console.WriteLine("  " + a + PathBase);
            }

            console.ResetColor();
            console.WriteLine("");
            console.WriteLine("Press CTRL+C to exit");

            if (OpenBrowser)
            {
                var url = addresses.Addresses.First();
                // normalize to loopback if binding to IPAny
                url = url.Replace("0.0.0.0", "localhost");
                url = url.Replace("[::]", "localhost");

                if (!string.IsNullOrEmpty(PathBase))
                {
                    url += PathBase;
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
