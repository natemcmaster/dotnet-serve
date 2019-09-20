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
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Serve
{
    class SimpleServer
    {
        private readonly CommandLineOptions _options;
        private readonly IConsole _console;
        private readonly string _currentDirectory;
        private readonly IReporter _reporter;

        public SimpleServer(CommandLineOptions options, IConsole console, string currentDirectory)
        {
            _options = options;
            _console = console;
            _currentDirectory = currentDirectory;
            _reporter = new ConsoleReporter(console)
            {
                IsQuiet = options.Quiet,
                IsVerbose = options.Verbose,
            };
        }

        public async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            var directory = Path.GetFullPath(_options.Directory ?? _currentDirectory);
            var port = _options.Port;

            if (!CertificateLoader.TryLoadCertificate(_options, _currentDirectory, out var cert, out var certLoadError))
            {
                _reporter.Verbose(certLoadError.ToString());
                _reporter.Error(certLoadError.Message);
                return 1;
            }

            if (cert != null)
            {
                _reporter.Verbose($"Using certificate {cert.SubjectName.Name} ({cert.Thumbprint})");
            }

            void ConfigureHttps(ListenOptions options)
            {
                if (cert != null)
                {
                    options.UseHttps(cert);
                }
            }

            var host = new WebHostBuilder()
                .ConfigureLogging(l =>
                {
                    l.SetMinimumLevel(_options.MinLogLevel);
                    l.AddConsole();
                })
                .PreferHostingUrls(false)
                .UseKestrel(o =>
                {
                    if (_options.ShouldUseLocalhost())
                    {
                        o.ListenLocalhost(port, ConfigureHttps);
                    }
                    else
                    {
                        foreach (var a in _options.Addresses)
                        {
                            o.Listen(a, port, ConfigureHttps);
                        }
                    }
                })
                .UseWebRoot(directory)
                .UseContentRoot(directory)
                .UseEnvironment("Production")
                .SuppressStatusMessages(true)
                .UseStartup<Startup>()
                .ConfigureServices(s => s.AddSingleton(_options))
                .Build();

            _console.Write(ConsoleColor.DarkYellow, "Starting server, serving ");
            _console.WriteLine(Path.GetRelativePath(_currentDirectory, directory));

            var defaultExtensions = _options.GetDefaultExtensions();
            if (defaultExtensions != null)
            {
                _console.WriteLine(ConsoleColor.DarkYellow, $"Using default extensions " + string.Join(", ", defaultExtensions));
            }

            await host.StartAsync(cancellationToken);
            AfterServerStart(host);
            await host.WaitForShutdownAsync(cancellationToken);
            return 0;
        }

        private void AfterServerStart(IWebHost host)
        {
            var addresses = host.ServerFeatures.Get<IServerAddressesFeature>();
            var pathBase = _options.GetPathBase();

            _console.WriteLine("Listening on:");
            foreach (var a in addresses.Addresses)
            {
                _console.WriteLine(ConsoleColor.Green, "  " + a + pathBase);
            }

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
            var psi = new ProcessStartInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.ArgumentList.Add(url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                psi.FileName = "xdg-open";
                psi.ArgumentList.Add(url);
            }
            else
            {
                psi.FileName = "cmd";
                psi.ArgumentList.Add("/C");
                psi.ArgumentList.Add("start");
                psi.ArgumentList.Add(url);
            }

            Process.Start(psi);
        }
    }
}
