# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

dotnet-serve is a .NET global tool that provides a simple command-line HTTP server for serving static files. It's built on ASP.NET Core Kestrel and distributed via NuGet.

## Build Commands

```bash
# Build and test (recommended)
./build.ps1                           # Debug build
./build.ps1 -Configuration Release    # Release build
./build.ps1 -ci                       # CI mode: Release + format verification

# Individual commands
dotnet format                         # Format code (enforced in CI)
dotnet build                          # Build solution
dotnet test                           # Run tests with coverage
dotnet pack -o artifacts              # Create NuGet package
```

Build output goes to `artifacts/` for packages and `.build/` for intermediate files.

## Testing

```bash
dotnet test                                    # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # Run specific test
```

Test project uses xUnit. Test assets are in `test/dotnet-serve.Tests/TestAssets/`.

## Architecture

**Solution structure:**
- `src/dotnet-serve/` - Main CLI tool
- `test/dotnet-serve.Tests/` - Test suite

**Key source files:**
- `Program.cs` - Entry point, CLI setup using McMaster.Extensions.CommandLineUtils
- `CommandLineOptions.cs` - CLI argument definitions with DotNetConfig integration
- `SimpleServer.cs` - WebHost builder and server lifecycle
- `Startup.cs` - ASP.NET Core middleware pipeline configuration

**Middleware pipeline (Startup.cs):**
1. CORS handling (optional)
2. Response compression (gzip/brotli)
3. Reverse proxy routing (YARP-based)
4. Custom headers injection
5. Default file extensions handling
6. Static file serving with directory browsing

**Key dependencies:**
- McMaster.Extensions.CommandLineUtils - CLI parsing
- DotNetConfig - `.netconfig` file support
- Yarp.ReverseProxy - Reverse proxy functionality
- Portable.BouncyCastle - Certificate handling

**Configuration:** The tool reads from `.netconfig` files using hierarchical lookup and can save options back with `--save-options`.

## Code Style

- C# 10.0 with implicit usings enabled
- Code style enforcement enabled (warnings as errors)
- Run `dotnet format` before committing

## CI Skip Directives

- Include `ci skip` in commit message to skip the entire CI build
- Include `skip release` in commit message to skip the release job (build still runs)
