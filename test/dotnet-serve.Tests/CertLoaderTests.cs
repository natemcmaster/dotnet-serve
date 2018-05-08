// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Moq;
using Xunit;

namespace McMaster.DotNet.Serve.Tests
{
    public class CertLoaderTests
    {
        [Fact]
        public void ItLoadsCertPfxFileByDefault()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Https", "pfx");
            var options = new Mock<CommandLineOptions>();
            options.SetupGet(o => o.Directory).Returns(path);
            options.SetupGet(o => o.CertificatePassword).Returns("testPassword");
            options.SetupGet(o => o.UseTls).Returns(true);
            var x509 = CertificateLoader.LoadCertificate(options.Object);
            Assert.NotNull(x509);
            Assert.Equal("E8481D606B15080024C806EFE89B00F0976BD906", x509.Thumbprint);
            Assert.True(x509.HasPrivateKey, "Cert should have private key");
        }
    }
}
