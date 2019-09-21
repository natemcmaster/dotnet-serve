# Changelog

See [unreleased] changes.

## [v1.4.1]

* Fix private key loading on .NET Core 3.0

## [v1.4.0]

* Add support for .NET Core 3.0

## [v1.3.0]

* [@SonicGD]: Add -z|--gzip option to enable output compression ([#23])

[#23]: https://github.com/natemcmaster/dotnet-serve/pull/23

## [v1.2.0]

* [@andrewlock]: Add support for setting custom response headers ([#21])

[#21]: https://github.com/natemcmaster/dotnet-serve/pull/21

## [v1.1.0]

* Remove `--razor`. This was a prototype, but due to pending deprecation of some Razor APIs, this feature won't work in the future anyways. See https://github.com/aspnet/Announcements/issues/312
* [@Tarmil]: Add support for setting custom MIME types base on file extension

## [v1.0.0]
- Update runtime to use ASP.NET Core 2.1.0.

## [v0.4.1]

- Add support for configuring HTTPs with .pem encoded certificates

## [v0.4.0]

- Add support for configuring HTTPs with .pfx/.p12 files
- Update runtime to use 2.1.0-rc1

## [v0.3.0]

Breaking change:
 - Change syntax to make the directory served a flag. `-d|--directory`
 - [@shanselman]: Make localhost the default address instead of 0.0.0.0
 - Update to ASP.NET Core 2.1.0-preview2

Enhancements:
 - [@daveaglick]: Add support for default extensions
 - Add support for Razor Pages (experimental. Must be enabled with `--razor`)

## [v0.2.1]

**March 28, 2018**

Enhancements:
 - Add `--path-base` option to support setting a root URL to postpend to the site URL

Fixes:
 - Launch the browser to localhost when 0.0.0.0 or [::] is used

## [v0.2.0]

**March 13, 2018**

Initial release of dotnet-serve as a global CLI tool.

Changes:
  - Release the package as a DotnetTool package (global CLI tool)
  - Drop support for installing as a DotNetCliToolReference
  - Update to ASP.NET Core 2.1.0-preview1-final

## [v0.1.0]
Initial release
 - Provides a simple command-line web server for dotnet

[unreleased]: https://github.com/natemcmaster/dotnet-serve/compare/v1.4.1...HEAD
[v1.4.1]: https://github.com/natemcmaster/dotnet-serve/compare/v1.4.0...v1.4.1
[v1.4.0]: https://github.com/natemcmaster/dotnet-serve/compare/v1.3.0...v1.4.0
[v1.3.0]: https://github.com/natemcmaster/dotnet-serve/compare/v1.2.0...v1.3.0
[v1.2.0]: https://github.com/natemcmaster/dotnet-serve/compare/v1.1.0...v1.2.0
[v1.1.0]: https://github.com/natemcmaster/dotnet-serve/compare/v1.0.0...v1.1.0
[v1.0.0]: https://github.com/natemcmaster/dotnet-serve/compare/v0.4.1...v1.0.0
[v0.4.1]: https://github.com/natemcmaster/dotnet-serve/compare/v0.4.0...v0.4.1
[v0.4.0]: https://github.com/natemcmaster/dotnet-serve/compare/v0.3.0...v0.4.0
[v0.3.0]: https://github.com/natemcmaster/dotnet-serve/compare/v0.2.1...v0.3.0
[v0.2.1]: https://github.com/natemcmaster/dotnet-serve/compare/v0.2.0...v0.2.1
[v0.2.0]: https://github.com/natemcmaster/dotnet-serve/compare/v0.1.0...v0.2.0
[v0.1.0]: https://github.com/natemcmaster/dotnet-serve/tree/v0.1.0

[@andrewlock]: https://github.com/andrewlock
[@daveaglick]: https://github.com/daveaglick
[@shanselman]: https://github.com/shanselman
[@SonicGD]: https://github.com/SonicGD
[@Tarmil]: https://github.com/Tarmil
