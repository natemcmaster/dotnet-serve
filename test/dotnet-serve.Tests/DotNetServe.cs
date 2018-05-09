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
        private static readonly string s_dotnetServe = Path.Combine(AppContext.BaseDirectory, "tool", "dotnet-serve.dll");
        private static int s_nextPort = 9000;

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
            bool enableRazor = false,
            bool enableTls = false,
            string certPassword = null,
            ITestOutputHelper output = null)
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

            if (enableRazor)
            {
                psi.ArgumentList.Add("--razor");
            }

            if (enableTls)
            {
                psi.ArgumentList.Add("--tls");
            }

            if (certPassword != null)
            {
                psi.ArgumentList.Add("--cert-pwd");
                psi.ArgumentList.Add(certPassword);
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
