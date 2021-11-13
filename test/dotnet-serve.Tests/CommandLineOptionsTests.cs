// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace McMaster.DotNet.Serve.Tests;

public class CommandLineOptionsTests
{
    [Fact]
    public void SplitsHeadersCorrectly()
    {
        var headers = new[]
        {
                "X-XSS-Protection: 1; mode=block",
                "x-frame-options:SAMEORIGIN",
            };
        var options = new CommandLineOptions();
        SetPropertyValue(options, nameof(CommandLineOptions.Headers), headers);

        var parsedHeaders = options.GetHeaders();
        var expectedHeaders = new Dictionary<string, string>
            {
                {"X-XSS-Protection", "1; mode=block" },
                {"x-frame-options", "SAMEORIGIN" },
            };

        Assert.Equal(expectedHeaders.Count, parsedHeaders.Count);

        foreach (var expectedHeader in expectedHeaders)
        {
            var parsedHeader = Assert.Single(parsedHeaders, h => h.Key == expectedHeader.Key);
            Assert.Equal(expectedHeader.Value, parsedHeader.Value);
        }
    }

    /// <summary>
    /// Helper method for setting the backing field for getter-only auto-properties
    /// </summary>
    private static void SetPropertyValue(CommandLineOptions options, string propertyName, object value)
    {
        var type = typeof(CommandLineOptions);
        var property = type.GetProperty(propertyName);

        if (property.CanWrite)
        {
            property.SetValue(options, value);
        }
        else
        {
            var backingField = type
              .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
              .First(field =>
                field.Attributes.HasFlag(FieldAttributes.Private) &&
                field.Attributes.HasFlag(FieldAttributes.InitOnly) &&
                field.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute)) &&
                (field.DeclaringType == property.DeclaringType) &&
                field.FieldType.IsAssignableFrom(property.PropertyType) &&
                field.Name.StartsWith("<" + property.Name + ">")
              );
            backingField.SetValue(options, value);
        }
    }
}
