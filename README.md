dotnet-serve
============

[![Travis build status][travis-badge]](https://travis-ci.org/natemcmaster/dotnet-serve/branches)
[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/natemcmaster/dotnet-serve/branch/master)

[travis-badge]: https://img.shields.io/travis/natemcmaster/dotnet-serve/master.svg?label=travis&style=flat-square
[appveyor-badge]: https://img.shields.io/appveyor/ci/natemcmaster/dotnet-serve/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/dotnet-serve/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-serve.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/natemcmaster/package/nuget/dotnet-serve
[main-myget-badge]: https://img.shields.io/www.myget/natemcmaster/vpre/dotnet-serve.svg?style=flat-square&label=myget

A simple command-line HTTP server.

It launches a server in the current working directory and serves all files in it.

## Get started

Download the .NET Core SDK [2.1.300](https://aka.ms/DotNetCore21) or newer.
Once installed, run this command:

```
dotnet tool install --global dotnet-serve
```

Start a simple server and open the browser by running

```
dotnet serve -o
```

..and with HTTPS.
```
dotnet serve -o -S
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
  --cert                             A PEM encoded certificate file to use for HTTPS connections.
                                     Defaults to file in current directory named 'cert.pem'
  --key                              A PEM encoded private key to use for HTTPS connections.
                                     Defaults to file in current directory named 'private.key'
  --pfx                              A PKCS#12 certificate file to use for HTTPS connections.
                                     Defaults to file in current directory named 'cert.pfx'
  --pfx-pwd                          The password to open the certificate file. (Optional)
  --razor                            Enable Razor Pages support (Experimental)
  -?|-h|--help                       Show help information
```

## Configuring HTTPS

`dotnet serve -S` will serve requests over HTTPS. By default, it will attempt to find an appropriate certificate
on the mcahine.

By default, `dotnet serve` will look for, in order:
 - A pair of files named `cert.pem` and `private.key` in the current directory
 - A file named `cert.pfx` in the current directory
 - The ASP.NET Core Developer Certificate (localhost only)

You can also manually specify certificates as command line options (see below):

> _See also [this doc](./docs/GenerateCert.md) for how to create a self-signed HTTPS certificate._

### .pem files

Use this when you have your certficate and private key stored in separate files (PEM encoded).
```
dotnet serve --cert ./cert.pem --key ./private.pem
```

Note: currently only RSA private keys are supported.

### .pfx file

You can generate a self-signed

Use this when you have your certficate as a .pfx/.p12 file (PKCS#12 format).
```
dotnet serve --pfx myCert.pfx --pfx-pwd certPass123
```

### Using the ASP.NET Core Developer Certificate

The developer certificate is automatically created the first time you use `dotnet`.
When serving on 'localhost', dotnet-serve will discover and use when you run:

```
dotnet serve -S
```
