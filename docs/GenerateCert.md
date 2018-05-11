Generating a self-signed certificate for HTTPS
----------------------------------------------

## Using openssl (macOS/Linux)

On macOS/Linux, you can generate a self-signed certificate by running:

    openssl req -x509 -days 365 -nodes -newkey rsa:2048 -keyout private.key -out cert.pem

This will store the certificate and key in separate files as ASN.1 objects in PEM encoded files.

To combine these into a single, password-protected file, run:

    openssl pkcs12 -export -in cert.pem -inkey private.key -out cert.pfx

This file is a PKCS#12 encoded file, aka P12 or PFX.

## Using PowerShell (Windows)

On Windows 10, you can generate a self-signed certificate by running:

    $cert = New-SelfSignedCertificate -DnsName localhost -CertStoreLocation cert:\CurrentUser\My
    Export-PfxCertificate $cert -FilePath cert.pfx -Password (ConvertTo-SecureString testPassword -Force -AsPlainText)

This file is a PKCS#12 encoded file, aka P12 or PFX.

## Generating the self-signed ASP.NET Core Developer certificate

You can generate the ASP.NET Core developer certificate by running:

```
dotnet dev-certs https
```

This will create the certificate and store it in the current user machine store.
