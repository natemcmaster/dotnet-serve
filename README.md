dotnet-serve
============

[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/natemcmaster/dotnet-serve/branch/master)

[appveyor-badge]: https://img.shields.io/appveyor/ci/natemcmaster/dotnet-serve/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/dotnet-serve/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-serve.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/natemcmaster/package/nuget/dotnet-serve
[main-myget-badge]: https://img.shields.io/www.myget/natemcmaster/vpre/dotnet-serve.svg?style=flat-square&label=myget

A simple command-line HTTP server.

It launches a server in the current working directory and serves all files in it.

## Installation

The latest release of dotnet-serve requires the [2.1.300-rc1](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300-rc1) .NET Core SDK or newer.
Once installed, run this command:

```
dotnet tool install --global dotnet-serve
```

## Usage

```
Usage: dotnet serve [options]

Options:
  --version                          Show version information
  -d|--directory <DIR>               The root directory to serve. [Current directory]
  -o|--open-browser                  Open a web browser when the server starts. [false]
  -p|--port <PORT>                   Port to use [8080]. Use 0 for a dynamic port.
  -a|--address <ADDRESS>             Address to use [127.0.0.1]
  --path-base <PATH>                 The base URL path of postpended to the site url.
  --default-extensions:<EXTENSIONS>  A comma-delimited list of extensions to use when no extension is provided in the URL. [.html,.htm]
  -q|--quiet                         Show less console output.
  -v|--verbose                       Show more console output.
  -S|--tls                           Enable TLS (HTTPS)
  --pfx                              A PKCS#12 certificate file to use for HTTPS connections.
                                     Defaults to file in current directory named 'cert.pfx'
  --pfx-pwd                          The password to open the certificate file. (Optional)
  --razor                            Enable Razor Pages support (Experimental)
  -?|-h|--help                       Show help information
```

## Configuring HTTPS

`dotnet-serve` supports serving requests over HTTPS. You can configure the certificates used for HTTPS in the
following ways.

### .pem files

Use this when you have your certficate and private key stored in separate files (PEM encoded).
```
dotnet serve --cert ./cert.pem --key ./private.pem
```

Note: currently only RSA private keys are supported.

### .pfx file

Use this when you have your certficate as a .pfx/.p12 file (PKCS#12 format).
```
dotnet serve --pfx myCert.pfx --pfx-pwd certPass123
```

### Using the ASP.NET Core Developer Certificate

You can generated an install the ASP.NET Core developer certificate by running

```
dotnet dev-certs https
```

Then launch `dotnet-serve` as
```
dotnet serve -S
```

### Defaults

If you just run `dotnet serve -S`, it will attempt to find a .pfx or ASP.NET Core dev cert automatically.

It will look for, in order:
 - A pair of files named `cert.pem` and `private.key` in the current directory
 - A file named `cert.pfx` in the current directory
 - The ASP.NET Core Developer Certificate
