// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    class DotNetServe : IDisposable
    {
        private const string TargetFramework
#if NETCOREAPP2_1
            = "netcoreapp2.1";
#elif NETCOREAPP3_0
            = "netcoreapp3.0";
#else
#error Update target frameworks
#endif

        private static readonly string s_dotnetServe
            = Path.Combine(AppContext.BaseDirectory, "tool", TargetFramework, "dotnet-serve.dll");

        private static int s_nextPort
#if NETCOREAPP2_1 // avoid conflicts if tests for both target frameworks run at the same time.
            = 9000;
#elif NETCOREAPP3_0
            = 10000;
#else
#error Update target frameworks
#endif

        private readonly Process _process;
        private readonly ITestOutputHelper _output;
        private readonly SemaphoreSlim _outputReceived = new SemaphoreSlim(0);
        private int _port;

        private DotNetServe(Process process, int port, bool useHttps, ITestOutputHelper output)
        {
            _process = process;
            _port = port;
            _output = output;
            var protocol = useHttps
                ? "https"
                : "http";

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true,
            };
            Client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{protocol}://localhost:{port}"),
            };
            process.OutputDataReceived += HandleOutput;
            process.ErrorDataReceived += HandleOutput;
            process.Exited += HandleExit;
        }

        public HttpClient Client { get; }

        public void Start()
        {
            if (_output != null)
            {
                _output.WriteLine($"Starting: {_process.StartInfo.FileName} {ArgumentEscaper.EscapeAndConcatenate(_process.StartInfo.ArgumentList)}");
            }
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _outputReceived.Wait(TimeSpan.FromSeconds(5));
        }

        private void HandleExit(object sender, EventArgs e)
        {
            _outputReceived.Release();
            if (_output != null)
            {
                _output.WriteLine($"dotnet-serve exited {_process.ExitCode}");
            }
        }

        private void HandleOutput(object sender, DataReceivedEventArgs e)
        {
            _outputReceived.Release();
            if (e.Data != null && _output != null)
            {
                _output.WriteLine("dotnet-serve: " + e.Data);
            }
        }

        public void Dispose()
        {
            Client.Dispose();

            if (!_process.HasExited)
            {
                _process.Kill();
            }

            _process.Dispose();
            _process.OutputDataReceived -= HandleOutput;
            _process.ErrorDataReceived -= HandleOutput;
            _process.Exited -= HandleExit;
        }

        public static DotNetServe Start(
            string directory = null,
            int? port = default,
            bool enableTls = false,
            string certPassword = null,
            string[] mimeMap = null,
            string[] headers = null,
            ITestOutputHelper output = null,
            bool useGzip = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = DotNetExe.FullPathOrDefault(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                ArgumentList =
                {
                    s_dotnetServe,
                    "--verbose",
                },
                WorkingDirectory = directory ?? AppContext.BaseDirectory,
            };

            if (directory != null)
            {
                psi.ArgumentList.Add("-d");
                psi.ArgumentList.Add(directory);
            }

            if (!port.HasValue)
            {
                port = Interlocked.Increment(ref s_nextPort);
            }

            psi.ArgumentList.Add("-p");
            psi.ArgumentList.Add(port.ToString());

            if (enableTls)
            {
                psi.ArgumentList.Add("--tls");
            }

            if (certPassword != null)
            {
                psi.ArgumentList.Add("--pfx-pwd");
                psi.ArgumentList.Add(certPassword);
            }

            if (mimeMap != null)
            {
                foreach (var mapping in mimeMap)
                {
                    psi.ArgumentList.Add("-m");
                    psi.ArgumentList.Add(mapping);
                }
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    psi.ArgumentList.Add("-h");
                    psi.ArgumentList.Add(header);
                }
            }

            if (useGzip)
            {
                psi.ArgumentList.Add("-z");
            }

            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = psi,
            };

            var serve = new DotNetServe(process, port.Value, enableTls, output);
            serve.Start();
            return serve;
        }
    }
}
