dotnet-serve
============

[![Build Status][ci-badge]][ci] [![Code Coverage][codecov-badge]][codecov]
[![NuGet][nuget-badge] ![NuGet Downloads][nuget-download-badge]][nuget]

[ci]: https://github.com/natemcmaster/dotnet-serve/actions?query=workflow%3ACI+branch%3Amain
[ci-badge]: https://github.com/natemcmaster/dotnet-serve/workflows/CI/badge.svg
[codecov]: https://codecov.io/gh/natemcmaster/dotnet-serve
[codecov-badge]: https://codecov.io/gh/natemcmaster/dotnet-serve/branch/main/graph/badge.svg?token=l6uSsHZ8nA
[nuget]: https://www.nuget.org/packages/dotnet-serve/
[nuget-badge]: https://img.shields.io/nuget/v/dotnet-serve.svg?style=flat-square
[nuget-download-badge]: https://img.shields.io/nuget/dt/dotnet-serve?style=flat-square

A simple command-line HTTP server.

It launches a server in the current working directory and serves all files in it.

## Get started

[Install .NET 6 or newer](https://get.dot.net) and run this command:

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

... with a specific port (otherwise, it defaults to a random, unused port).
```
dotnet serve --port 8080
```

... with access allowed to remote machines (defaults to loopback only.) Use this if running inside Docker.

```
dotnet serve --address any
```

## Usage

```
Usage: dotnet serve [options]

Options:
  --version                            Show version information.
  -d|--directory <DIR>                 The root directory to serve. [Current directory]
  -o|--open-browser                    Open a web browser when the server starts. [false]
  -p|--port <PORT>                     Port to use [8080]. Use 0 for a dynamic port.
  -a|--address <ADDRESS>               Address to use. [Default = localhost].
                                       Accepts IP addresses,
                                       'localhost' for only accept requests from loopback connections, or
                                       'any' to accept requests from any IP address.
  --path-base <PATH>                   The base URL path of postpended to the site url.
  --reverse-proxy <MAPPING>            Map a path pattern to another url.
                                       Expected format is <SOURCE_PATH_PATTERN>=<DESTINATION_URL_PREFIX>.
                                       SOURCE_PATH_PATTERN uses ASP.NET routing syntax. Use {**all} to match anything.
  --default-extensions[:<EXTENSIONS>]  A comma-delimited list of extensions to use when no extension is provided in the URL. [.html,.htm]
  -q|--quiet                           Show less console output.
  -v|--verbose                         Show more console output.
  -h|--headers <HEADER_AND_VALUE>      A header to return with all file/directory responses. e.g. -h "X-XSS-Protection: 1; mode=block"
  -S|--tls                             Enable TLS (HTTPS)
  --cert                               A PEM encoded certificate file to use for HTTPS connections.
                                       Defaults to file in current directory named 'cert.pem'
  --key                                A PEM encoded private key to use for HTTPS connections.
                                       Defaults to file in current directory named 'private.key'
  --pfx                                A PKCS#12 certificate file to use for HTTPS connections.
                                       Defaults to file in current directory named 'cert.pfx'
  --pfx-pwd                            The password to open the certificate file. (Optional)
  -m|--mime <MAPPING>                  Add a mapping from file extension to MIME type. Empty MIME removes a mapping.
                                       Expected format is <EXT>=<MIME>.
  -z|--gzip                            Enable gzip compression
  -b|--brotli                          Enable brotli compression
  -c|--cors                            Enable CORS (It will enable CORS for all origin and all methods)
  --save-options                       Save specified options to .netconfig for subsequent runs.
  --config-file                          Use the given .netconfig file.
  --fallback-file                       The path to a file which is served for requests that do not match known file names.
                                       This is commonly used for single-page web applications.
  -?|--help                            Show help information.
```

> Tip: single letters for options can be combined. Example: `dotnet serve -Sozq`

## Configuring HTTPS

`dotnet serve -S` will serve requests over HTTPS. By default, it will attempt to find an appropriate certificate
on the machine.

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

## Reverse Proxy

`dotnet-serve --reverse-proxy /api/{**all}=http://localhost:5000`
will proxy all requests matching `/api/*` to `http://localhost:5000/api/*`.

The source path pattern uses ASP.NET routing syntax.
[See the ASP.NET docs for more info.](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0#route-template-reference)

Multiple `--reverse-proxy <MAPPING>` directives can be defined.

## Reusing options with .netconfig

`dotnet-serve` supports reading and saving options using [dotnet-config](https://dotnetconfig.org/),
which provides hierarchical inherited configuration for any .NET tool. This means you can save your
frequently used options to `.netconfig` so you don't need to specify them every time and for every
folder you serve across your machine.

To save the options used in a particular run to the current directory's `.netconfig`, just append
`--save-options`:

```
dotnet serve -p 8080 --gzip --cors --quiet --save-options
```

After running that command, a new `.netconfig` will be created (if there isn't one already there)
with the following section for `dotnet-serve`:

```ini
[serve]
	port = 8000
	quiet
	gzip
	cors
	header = X-My-Option: foo
	header = X-Another: bar
```

(note multiple `header`, `mime` type mappings and `exclude-file` entries can be provided as
individual variables)

You can place those settings in any parent folder and it will be reused across all descendent
folders, or they can also be saved to the global (user profile) or system locations. To easily
configure these options at those levels, use the `dotnet-config` tool itself:

```
dotnet config --global --set serve.port 8000
```

This will default the port to `8000` whenever a port is not specified in the command line. You
can open the saved `.netconfig` at `%USERPROFILE%\.netconfig` or `~/.netconfig`.

The `cert`, `key` and `pfx` values, in particular, can be relative paths that are resolved
relative to the location of the declaring `.netconfig` file, which can be very convenient.
