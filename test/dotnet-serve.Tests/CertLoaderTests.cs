// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace McMaster.DotNet.Serve.Tests
{
    public class CertLoaderTests
    {
        private readonly ITestOutputHelper _output;

        public CertLoaderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ItLoadsCertPfxFileByDefault()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", "pfx");
            var options = new Mock<CommandLineOptions>();
            options.SetupGet(o => o.CertificatePassword).Returns("testPassword");
            options.SetupGet(o => o.UseTls).Returns(true);
            Assert.True(CertificateLoader.TryLoadCertificate(options.Object, path, out var x509, out _));
            Assert.NotNull(x509);
            Assert.Equal("E8481D606B15080024C806EFE89B00F0976BD906", x509.Thumbprint);
            Assert.True(x509.HasPrivateKey, "Cert should have private key");
        }

        [Fact]
        public async Task ItDoesNotServePfxFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", "pfx");
            using var ds = DotNetServe.Start(path,
                certPassword: "testPassword",
                output: _output,
                enableTls: true);
            var resp = await ds.Client.GetWithRetriesAsync("/cert.pfx");
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Theory]
        [InlineData("rsa", "E8481D606B15080024C806EFE89B00F0976BD906")]
        public void ItLoadsPemAndKeyFileByDefault(string keyFormat, string thumbprint)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", keyFormat);
            var options = new Mock<CommandLineOptions>();
            options.SetupGet(o => o.UseTls).Returns(true);
            Assert.True(CertificateLoader.TryLoadCertificate(options.Object, path, out var x509, out _));
            Assert.NotNull(x509);
            Assert.Equal(thumbprint, x509.Thumbprint);
            Assert.True(x509.HasPrivateKey, "Cert should have private key");
        }

        [Theory]
        [InlineData("rsa")]
        public async Task ItDoesNotServePemFiles(string keyFormat)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", keyFormat);
            using var ds = DotNetServe.Start(path,
                output: _output,
                enableTls: true);
            var resp1 = await ds.Client.GetWithRetriesAsync("/cert.pem");
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var resp2 = await ds.Client.GetWithRetriesAsync("/private.key");
            Assert.Equal(HttpStatusCode.Forbidden, resp2.StatusCode);
        }
    }
}
