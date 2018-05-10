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
            this._output = output;
        }

        [Fact]
        public void ItLoadsCertPfxFileByDefault()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", "pfx");
            var options = new Mock<CommandLineOptions>();
            options.SetupGet(o => o.CertificatePassword).Returns("testPassword");
            options.SetupGet(o => o.UseTls).Returns(true);
            var x509 = CertificateLoader.LoadCertificate(options.Object, path);
            Assert.NotNull(x509);
            Assert.Equal("E8481D606B15080024C806EFE89B00F0976BD906", x509.Thumbprint);
            Assert.True(x509.HasPrivateKey, "Cert should have private key");
        }

        [Fact]
        public async Task ItDoesNotServePfxFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", "pfx");
            using (var ds = DotNetServe.Start(path,
                certPassword: "testPassword",
                output: _output,
                enableTls: true))
            {
                var resp = await ds.Client.GetWithRetriesAsync("/cert.pfx");
                Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
            }
        }
    }
}
