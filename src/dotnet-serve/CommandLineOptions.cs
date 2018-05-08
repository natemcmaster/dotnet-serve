// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Serve
{
    [Command(
        Name = "dotnet serve",
        FullName = "dotnet-serve",
        Description = "A simple command-line HTTP server")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class CommandLineOptions
    {
        private LogLevel? _logLevel;
        private bool? _useTls;

        [Option(Description = "The root directory to serve. [Current directory]")]
        [DirectoryExists]
        public virtual string Directory { get; } = System.IO.Directory.GetCurrentDirectory();

        [Option(Description = "Open a web browser when the server starts. [false]")]
        public bool OpenBrowser { get; }

        [Option(Description = "Port to use [8080]. Use 0 for a dynamic port.")]
        [Range(0, 65535, ErrorMessage = "Invalid port. Ports must be in the range of 0 to 65535.")]
        public int Port { get; } = 8080;

        [Option("-a|--address <ADDRESS>", Description = "Address to use [127.0.0.1]")]
        public IPAddress[] Addresses { get; }

        [Option("--path-base <PATH>", Description = "The base URL path of postpended to the site url.")]
        private string PathBase { get; }

        [Option("--default-extensions:<EXTENSIONS>", CommandOptionType.SingleOrNoValue, Description = "A comma-delimited list of extensions to use when no extension is provided in the URL. [.html,.htm]")]
        public (bool HasValue, string Extensions) DefaultExtensions { get; }

        [Option(Description = "Show less console output.")]
        public bool Quiet { get; }

        [Option(Description = "Show more console output.")]
        public bool Verbose { get; }

        [Option("--log <LEVEL>", Description = "For advanced diagnostics.", ShowInHelpText = false)]
        public LogLevel MinLogLevel
        {
            get
            {
                if (_logLevel.HasValue)
                {
                    return _logLevel.Value;
                }

                if (Quiet)
                {
                    return LogLevel.Error;
                }

                if (Verbose)
                {
                    return LogLevel.Debug;
                }

                return LogLevel.Information;
            }
            private set => _logLevel = value;
        }

        [Option("-S|--tls", Description = "Enable TLS (HTTPS)")]
        public virtual bool UseTls
        {
            get
            {
                if (_useTls.HasValue)
                {
                    return _useTls.Value;
                }

                return !string.IsNullOrEmpty(CertificatePath);
            }
            private set => _useTls = value;
        }

        [Option("--cert", Description = "A PKCS#12 certificate file to use for HTTPS connections.\nDefaults to file in current directory named '" + CertificateLoader.DefaultCertFileName + "'")]
        [FileExists]
        public string CertificatePath { get; }

        [Option("--cert-pwd", Description = "The password to open the certificate file. (Optional)")]
        public virtual string CertificatePassword { get; }

        [Option("--razor", Description = "Enable Razor Pages support (Experimental)")]
        public bool EnableRazor { get; }

        public string GetPathBase()
        {
            if (string.IsNullOrEmpty(PathBase))
            {
                return PathBase;
            }
            var pathBase = PathBase.Replace('\\', '/').TrimEnd('/');
            return pathBase[0] != '/' ? "/" + pathBase : pathBase;
        }

        public bool ShouldUseLocalhost()
            => Addresses == null
            || Addresses.Length == 0
            || (Addresses.Length == 1 && IPAddress.IsLoopback(Addresses[0]));

        public string[] GetDefaultExtensions() =>
            DefaultExtensions.HasValue
                ? string.IsNullOrEmpty(DefaultExtensions.Extensions)
                    ? new[] { ".html", ".htm" }
                    : DefaultExtensions.Extensions.Split(',').Select(x => x.StartsWith('.') ? x : "." + x).ToArray()
                : null;

        private static string GetVersion()
            => typeof(CommandLineOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
