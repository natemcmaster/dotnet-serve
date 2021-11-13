// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Net;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace McMaster.DotNet.Serve;

internal class IPAddressParser : IValueParser<IPAddress>
{
    public Type TargetType => typeof(IPAddress);

    public IPAddress Parse(string argName, string value, CultureInfo culture)
    {
        if (string.Equals("localhost", value, StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Loopback;
        }

        if (!IPAddress.TryParse(value, out var address))
        {
            throw new FormatException($"'{value}' is not a valid IP address");
        }
        return address;
    }

    object IValueParser.Parse(string argName, string value, CultureInfo culture)
        => Parse(argName, value, culture);
}
