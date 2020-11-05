// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using DotNetConfig;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace McMaster.DotNet.Serve
{
    class Program
    {
        public static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);
            return new Program().Run(args);
        }

        internal int Run(params string[] args)
        {
            try
            {
                using var app = new CommandLineApplication<CommandLineOptions>();
                app.ValueParsers.Add(new IPAddressParser());
                app.Conventions.UseDefaultConventions();
                app.OnExecuteAsync(async ct =>
                {
                    // NOTE: this isn't very elegant, but moving this logic
                    // down to the CommandLineUtils level proved quite challenging
                    // and potentially not that useful.
                    var config = Config.Build(app.Model.ConfigFile).GetSection("serve");

                    ReadConfig(config, app.Model);
                    if (app.Model.SaveOptions)
                    {
                        WriteConfig(config, app.Model);
                    }

                    return await OnRunAsync(app.Model, ct);
                });
                app.OnValidationError(r => Error(r.ErrorMessage));
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Error("Unexpected error: " + ex.ToString());
                return 2;
            }
        }

        public List<string> Errors { get; } = new List<string>();

        void Error(string message)
        {
            Errors.Add(message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        protected virtual Task<int> OnRunAsync(CommandLineOptions options, CancellationToken ct)
        {
            var server = new SimpleServer(options, PhysicalConsole.Singleton, Directory.GetCurrentDirectory());
            return server.RunAsync(ct);
        }

        static void ReadConfig(ConfigSection config, CommandLineOptions model)
        {
            model.Port ??= (int?)config.GetNumber("port");
            model.Directory ??= config.GetString("directory");
            model.OpenBrowser ??= config.GetBoolean("open-browser"); 
            model.Quiet ??= config.GetBoolean("quiet");
            model.Verbose ??= config.GetBoolean("verbose");
            model.CertPemPath ??= config.GetNormalizedPath("cert");
            model.PrivateKeyPath ??= config.GetNormalizedPath("key");
            model.CertPfxPath ??= config.GetNormalizedPath("pfx");
            model.CertificatePassword ??= config.GetString("pfx-pwd");
            model.UseGzip ??= config.GetBoolean("gzip");
            model.UseBrotli ??= config.GetBoolean("brotli");
            model.EnableCors ??= config.GetBoolean("cors");
            model.PathBase ??= config.GetString("path-base");

            if (!model.UseTlsSpecified && config.TryGetBoolean("tls", out var tls))
            {
                model.UseTls = tls;
            }

            var headers = config.GetAll("header").Select(e => e.RawValue).ToArray();
            if (headers.Length > 0)
            {
                if (model.Headers == null)
                {
                    model.Headers = headers;
                }
                else
                {
                    var all = new string[model.Headers.Length + headers.Length];
                    model.Headers.CopyTo(all, 0);
                    headers.CopyTo(all, model.Headers.Length);
                    model.Headers = all;
                }
            }

            var mime = config.GetAll("mime").Select(e => e.RawValue).ToArray();
            if (mime.Length > 0)
            {
                if (model.MimeMappings == null)
                {
                    model.MimeMappings = mime;
                }
                else
                {
                    var all = new string[model.MimeMappings.Length + mime.Length];
                    model.MimeMappings.CopyTo(all, 0);
                    mime.CopyTo(all, model.MimeMappings.Length);
                    model.MimeMappings = all;
                }
            }

            model.ExcludedFiles.AddRange(
                config.GetAll("exclude-file").Select(x => x.RawValue));
        }

        static void WriteConfig(ConfigSection config, CommandLineOptions model)
        {
            if (model.Port != null)
            {
                config.SetNumber("port", model.Port.GetValueOrDefault());
            }
            if (model.Directory != null)
            {
                config.SetString("directory", model.Directory);
            }
            if (model.OpenBrowser != null)
            {
                config.SetBoolean("open-browser", model.OpenBrowser.Value);
            }
            if (model.Quiet != null)
            {
                config.SetBoolean("quiet", model.Quiet.Value);
            }
            if (model.Verbose != null)
            {
                config.SetBoolean("verbose", model.Verbose.Value);
            }
            if (model.CertPemPath != null)
            {
                config.SetString("cert", model.CertPemPath);
            }
            if (model.PrivateKeyPath != null)
            {
                config.SetString("key", model.PrivateKeyPath);
            }
            if (model.CertPfxPath != null)
            {
                config.SetString("pfx", model.CertPfxPath);
            }
            if (model.CertificatePassword != null)
            {
                config.SetString("pfx-pwd", model.CertificatePassword);
            }
            if (model.UseGzip != null)
            {
                config.SetBoolean("gzip", model.UseGzip.Value);
            }
            if (model.UseBrotli != null)
            {
                config.SetBoolean("brotli", model.UseBrotli.Value);
            }
            if (model.EnableCors != null)
            {
                config.SetBoolean("cors", model.EnableCors.Value);
            }

            if (model.Headers != null)
            {
                foreach (var header in model.Headers)
                {
                    config.AddString("header", header);
                }
            }

            if (model.MimeMappings != null)
            {
                foreach (var mime in model.MimeMappings)
                {
                    config.AddString("mime", mime);
                }
            }

            foreach (var excluded in model.ExcludedFiles)
            {
                config.AddString("exclude-file", excluded);
            }
        }
    }
}
