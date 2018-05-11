// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;

namespace McMaster.DotNet.Serve
{
    class Program
    {
        // Return codes
        private const int ERROR = 2;
        private const int OK = 0;

        public static int Main(string[] args)
        {
            HandleDebugSwitch(ref args);

            try
            {
                var app = new CommandLineApplication<CommandLineOptions>();
                app.ValueParsers.Add(new IPAddressParser());
                app.Conventions.UseDefaultConventions();
                app.OnExecute(async () =>
                {
                    var server = new SimpleServer(app.Model, PhysicalConsole.Singleton, Directory.GetCurrentDirectory());
                    return await server.RunAsync();
                });
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unexpected error: " + ex.ToString());
                Console.ResetColor();
                return ERROR;
            }
        }

        [Conditional("DEBUG")]
        public static void HandleDebugSwitch(ref string[] args)
        {
            if (args.Length > 0 && string.Equals("--debug", args[0], StringComparison.OrdinalIgnoreCase))
            {
                args = args.Skip(1).ToArray();
                if (Debugger.IsAttached)
                {
                    return;
                }

                Console.WriteLine("Waiting for debugger to attach.");
                Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");

                const int interval = 250;
                var maxWait = 30 * 1000 / interval;
                while (!Debugger.IsAttached && maxWait > 0)
                {
                    maxWait--;
                    Thread.Sleep(TimeSpan.FromMilliseconds(interval));
                }

                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("Timed out waiting for 30 seconds for debugger to attach. Continuing execution.");
                }
            }
        }
    }
}
