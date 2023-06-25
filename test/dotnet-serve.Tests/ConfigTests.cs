// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

public class ConfigTests
{
    [Fact]
    public void ConfigurationProvidesDefaultOptions()
    {
        var certFile = Path.GetTempFileName();
        var keyFile = Path.GetTempFileName();
        var pfxFile = Path.GetTempFileName();
        var configFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    port = 4242
    directory = {Path.GetTempPath().Replace("\\", "\\\\")}
    open-browser
    quiet = true
    verbose = on
    cert = {certFile.Replace("\\", "\\\\")}
    key = {keyFile.Replace("\\", "\\\\")}
    pfx = {pfxFile.Replace("\\", "\\\\")}
    pfx-pwd = password
    gzip
    cors = yes
    header = X-H1: value
    header = X-H2: value
    mime = .cs=text/plain
    mime = .vb=text/plain
    mime = .fs=text/plain
    exclude-file = app.config
    exclude-file = appsettings.json
    reverse-proxy = /api/{{**all}}=http://localhost:5000
    reverse-proxy = /myPath\\=example=https://example.com # Note that '\' must be doubled in dotnet-config...
    path-base = foo
    fallback-file = fallback.html
");

        var program = new TestProgram();
        program.Run("--config-file", configFile);

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Equal(4242, options.Port);
        Assert.Equal(Path.GetTempPath(), options.Directory);
        Assert.True(options.OpenBrowser.hasValue);
        Assert.True(options.Quiet);
        Assert.True(options.Verbose);
        Assert.Equal(certFile, options.CertPemPath);
        Assert.Equal(keyFile, options.PrivateKeyPath);
        Assert.Equal(pfxFile, options.CertPfxPath);
        Assert.Equal("password", options.CertificatePassword);
        Assert.True(options.UseGzip);
        Assert.True(options.EnableCors);
        Assert.Equal("foo", options.PathBase);
        Assert.Equal("fallback.html", options.FallbackFile);

        Assert.Contains("X-H1: value", options.Headers);
        Assert.Contains("X-H2: value", options.Headers);

        Assert.Contains(".cs=text/plain", options.MimeMappings);
        Assert.Contains(".vb=text/plain", options.MimeMappings);
        Assert.Contains(".fs=text/plain", options.MimeMappings);

        Assert.Contains("app.config", options.ExcludedFiles);
        Assert.Contains("appsettings.json", options.ExcludedFiles);

        Assert.Contains("/api/{**all}=http://localhost:5000", options.ReverseProxyMappings);
        Assert.Contains("/myPath\\=example=https://example.com", options.ReverseProxyMappings);
    }

    [Fact]
    public void CommandLineOverridesConfiguration()
    {
        var certFile = Path.GetTempFileName();
        var keyFile = Path.GetTempFileName();
        var pfxFile = Path.GetTempFileName();
        var configFile = Path.GetTempFileName();
        var fallbackFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    port = 2424
    directory = {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\", "\\\\")}
    open-browser = false
    quiet = false
    verbose = false
    cert = cert
    key = key
    pfx = pfx
    pfx-pwd = pwd
    gzip = 0
    cors = no
    fallback-file = fallback-file.html
");

        var program = new TestProgram();
        program.Run(
            "--config-file", configFile,
            "-p", "4242",
            "-d", Path.GetTempPath(),
            "--cert", certFile,
            "--key", keyFile,
            "--pfx", pfxFile,
            "--pfx-pwd", "password",
            "-oqvzc",
            "--fallback-file", fallbackFile);

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Equal(4242, options.Port);
        Assert.Equal(Path.GetTempPath(), options.Directory);
        Assert.True(options.OpenBrowser.hasValue);
        Assert.True(options.Quiet);
        Assert.True(options.Verbose);
        Assert.Equal(certFile, options.CertPemPath);
        Assert.Equal(keyFile, options.PrivateKeyPath);
        Assert.Equal(pfxFile, options.CertPfxPath);
        Assert.Equal(fallbackFile, options.FallbackFile);
        Assert.Equal("password", options.CertificatePassword);
        Assert.True(options.UseGzip);
        Assert.True(options.EnableCors);
    }

    [Fact]
    public void ConfigurationHeadersAugmentOptions()
    {
        var configFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    header = X-H1: value
");


        var program = new TestProgram();
        program.Run("--config-file", configFile, "-h", "X-H2: value");

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Contains("X-H1: value", options.Headers);
        Assert.Contains("X-H2: value", options.Headers);
    }

    [Fact]
    public void ConfigurationMimeAugmentOptions()
    {
        var configFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    mime = .vb=text/plain
    mime = .fs=text/plain
");

        var program = new TestProgram();
        program.Run("--config-file", configFile, "-m", ".cs=text/plain");

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Contains(".cs=text/plain", options.MimeMappings);
        Assert.Contains(".vb=text/plain", options.MimeMappings);
        Assert.Contains(".fs=text/plain", options.MimeMappings);
    }

    [Fact]
    public void ConfigurationExcludedFilesAugmentOptions()
    {
        var configFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    exclude-file = app.config
");

        var program = new TestProgram();
        program.Run("--config-file", configFile, "--exclude-file", "appsettings.json");

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Contains("app.config", options.ExcludedFiles);
        Assert.Contains("appsettings.json", options.ExcludedFiles);
    }

    [Fact]
    public void ConfigurationReverseProxiesAugmentOptions()
    {
        var configFile = Path.GetTempFileName();

        File.WriteAllText(configFile, @$"
[config]
    root

[serve]
    reverse-proxy = /api/{{**all}}=http://localhost:5000
");

        var program = new TestProgram();
        program.Run("--config-file", configFile, "--reverse-proxy", "/myPath\\=example=https://example.com");

        var options = program.Options;

        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Contains("/api/{**all}=http://localhost:5000", options.ReverseProxyMappings);
        Assert.Contains("/myPath\\=example=https://example.com", options.ReverseProxyMappings);
    }

    [Theory]
    [InlineData("/api/{**all}=http://localhost:5000", "/api/{**all}", "http://localhost:5000")]
    [InlineData("/myPath\\=example=https://example.com", "/myPath=example", "https://example.com")]
    [InlineData("/myPath\\=example=https://example.com/another\\=equals", "/myPath=example", "https://example.com/another=equals")]
    [InlineData("/myPath=https://example.com/another\\=equals", "/myPath", "https://example.com/another=equals")]
    public void GetReverseProxyMappingsParseEscapedEquals(string rawProxyMapping, string expectedSourcePath, string expectedDestinationUrlPrefix)
    {
        var program = new TestProgram();
        program.Run("--reverse-proxy", rawProxyMapping);

        var options = program.Options;
        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        var parsedMappings = options.GetReverseProxyMappings();
        Assert.True(parsedMappings.Count == 1);

        Assert.Equal(expectedSourcePath, parsedMappings.Single().Key);
        Assert.Equal(expectedDestinationUrlPrefix, parsedMappings.Single().Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("=")]
    [InlineData("=hello")]
    [InlineData("hello=")]
    [InlineData("hello=hell=o")]
    [InlineData(" ")]
    [InlineData(" = ")]
    [InlineData(" =hello")]
    [InlineData("hello= ")]
    public void GetReverseProxyMappingsThrowsOnInvalidInput(string rawProxyMapping)
    {
        var program = new TestProgram();
        program.Run("--reverse-proxy", rawProxyMapping);

        var options = program.Options;
        Assert.True(options != null, string.Join(Environment.NewLine, program.Errors));

        Assert.Throws<ArgumentException>(() => options.GetReverseProxyMappings());
    }

    private class TestProgram : Program
    {
        protected override Task<int> OnRunAsync(CommandLineOptions options, CancellationToken ct)
        {
            Options = options;
            return Task.FromResult(0);
        }

        public CommandLineOptions Options { get; private set; }
    }
}
