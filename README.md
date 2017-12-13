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

Add this to your \*.csproj file and execute `dotnet restore`.

```xml
<ItemGroup>
  <DotNetCliToolReference Include="dotnet-serve" Version="0.1.0" />
</ItemGroup>
```

## Usage

```
dotnet serve [path] [options]

Arguments:
  path                    Base path to the server root

Options:
  -p|--port <PORT>        Port to use [8080]. Use 0 for a dynamic port.
  -a|--address <ADDRESS>  Address to use [0.0.0.0]
  -o|--open-browser       Open a web browser when the server starts [false]
  -?|-h|--help            Show help information
```
