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

The latest release of dotnet-serve requires the [2.1.300-preview1](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300-preview1) .NET Core SDK or newer.
Once installed, run this command:

```
dotnet install tool --global dotnet-serve
```

## Usage

```
Usage: dotnet serve [options]

Options:
  --version                          Show version information
  -?|-h|--help                       Show help information
  -d|--directory <DIRECTORY>         The root directory to serve. [Current directory]
  -o|--open-browser                  Open a web browser when the server starts. [false]
  -p|--port <PORT>                   Port to use [8080]. Use 0 for a dynamic port.
  -a|--address <ADDRESS>             Address to use [0.0.0.0]
  --path-base <PATH>                 The base URL path of postpended to the site url.
  --default-extensions:<EXTENSIONS>  A comma-delimited list of extensions to use when no extension is provided in the URL. [.html,.htm]
  -q|--quiet                         Show less console output.
  -v|--verbose                       Show more console output.
  --razor                            Enable Razor Pages support (Experimental)
```
