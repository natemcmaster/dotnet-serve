// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Serve;

internal class SimpleServer
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
            IsQuiet = options.Quiet == true,
            IsVerbose = options.Verbose == true,
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
                    if (port.GetValueOrDefault() == 0)
                    {
                        o.ListenAnyIP(0, ConfigureHttps);
                    }
                    else
                    {
                        o.ListenLocalhost(port.GetValueOrDefault(), ConfigureHttps);
                    }
                }
                else
                {
                    foreach (var a in _options.Addresses)
                    {
                        if (a == IPAddress.IPv6Any)
                        {
                            o.ListenAnyIP(port.GetValueOrDefault(), ConfigureHttps);
                        }
                        else
                        {
                            o.Listen(a, port.GetValueOrDefault(), ConfigureHttps);
                        }
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
            _console.WriteLine(ConsoleColor.DarkYellow,
                $"Using default extensions " + string.Join(", ", defaultExtensions));
        }

        await host.StartAsync(cancellationToken);
        var logger = host.Services.GetRequiredService<ILogger<SimpleServer>>();
        AfterServerStart(host, logger);
        await host.WaitForShutdownAsync(cancellationToken);
        return 0;
    }

    private void AfterServerStart(IWebHost host, ILogger<SimpleServer> logger)
    {
        var addresses = host.ServerFeatures.Get<IServerAddressesFeature>();
        var pathBase = _options.GetPathBase();

        _console.WriteLine(GetListeningAddressText(addresses));
        foreach (var a in addresses.Addresses)
        {
            logger.LogDebug("Listening on {address}", a);
            _console.WriteLine(ConsoleColor.Green, "  " + NormalizeToLoopbackAddress(a) + pathBase);
        }

        _console.WriteLine("");
        _console.WriteLine("Press CTRL+C to exit");

        if (_options.OpenBrowser.hasValue)
        {
            var uri = new Uri(NormalizeToLoopbackAddress(addresses.Addresses.First()));

            if (!string.IsNullOrWhiteSpace(_options.OpenBrowser.path))
            {
                uri = new Uri(uri, _options.OpenBrowser.path);
            }
            else if (!string.IsNullOrEmpty(pathBase))
            {
                uri = new Uri(uri, pathBase);
            }

            LaunchBrowser(uri.ToString());
        }

        static string GetListeningAddressText(IServerAddressesFeature addresses)
        {
            if (addresses.Addresses.Any())
            {
                var url = addresses.Addresses.First();
                if (url.Contains("0.0.0.0") || url.Contains("[::]"))
                {
                    return "Listening on any IP:";
                }
            }

            return "Listening on:";
        }

        static string NormalizeToLoopbackAddress(string url)
        {
            // normalize to loopback if binding to IPAny
            url = url.Replace("0.0.0.0", "localhost");
            url = url.Replace("[::]", "localhost");

            return url;
        }
    }

    private void LaunchBrowser(string url)
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            psi.FileName = "cmd";
            psi.ArgumentList.Add("/C");
            psi.ArgumentList.Add("start");
            psi.ArgumentList.Add(url);
        }
        else
        {
            _console.Write(ConsoleColor.Red, "Could not determine how to launch the browser for this OS platform.");
            return;
        }

        Process.Start(psi);
    }
}
