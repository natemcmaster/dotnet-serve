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
        private int _port;

        private DotNetServe(Process process, int port, ITestOutputHelper output)
        {
            _process = process;
            _port = port;
            _output = output;
            Client = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}"),
            };
            process.OutputDataReceived += HandleOutput;
            process.ErrorDataReceived += HandleOutput;
            process.Exited += HandleExit;
        }

        public HttpClient Client { get; }

        public void Start()
        {
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        private void HandleExit(object sender, EventArgs e)
        {
            if(_output != null)
            {
                _output.WriteLine($"dotnet-serve exited {_process.ExitCode}");
            }
        }

        private void HandleOutput(object sender, DataReceivedEventArgs e)
        {
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
            ITestOutputHelper output = null)
        {
            var args = new List<string>
            {
                s_dotnetServe
            };

            if (directory != null)
            {
                args.Add("-d");
                args.Add(directory);
            }

            if (!port.HasValue)
            {
                port = Interlocked.Increment(ref s_nextPort);
            }

            args.Add("-p");
            args.Add(port.ToString());

            if (enableRazor)
            {
                args.Add("--razor");
            }

            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = DotNetExe.FullPathOrDefault(),
                    Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            var serve = new DotNetServe(process, port.Value, output);
            serve.Start();
            return serve;
        }
    }
}
