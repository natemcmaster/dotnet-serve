// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace McMaster.DotNet.Serve
{
    class Program
    {
        public static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            try
            {
                using var app = new CommandLineApplication<CommandLineOptions>();
                app.ValueParsers.Add(new IPAddressParser());
                app.Conventions.UseDefaultConventions();
                app.OnExecuteAsync(async ct =>
                {
                    var server = new SimpleServer(app.Model, PhysicalConsole.Singleton, Directory.GetCurrentDirectory());
                    return await server.RunAsync(ct);
                });
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unexpected error: " + ex.ToString());
                Console.ResetColor();
                return 2;
            }
        }
    }
}
