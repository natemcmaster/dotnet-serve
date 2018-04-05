// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            try
            {
                var app = new CommandLineApplication<CommandLineOptions>();
                app.ValueParsers.Add(new IPAddressParser());
                app.Conventions.UseDefaultConventions();
                app.OnExecute(async () =>
                {
                    var server = new SimpleServer(app.Model, PhysicalConsole.Singleton);
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
    }
}
