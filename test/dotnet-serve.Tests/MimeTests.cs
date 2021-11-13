// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.DotNet.Serve.Tests;

public class MimeTests
{
    private readonly ITestOutputHelper _output;

    public MimeTests(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Theory]
    // Check that *.mytxt has no default mapping.
    [InlineData("/file.mytxt", "contents of file.mytxt\n", null, "notmytxt=application/x-notmytxt")]
    // Add a mapping without prefix dot.
    [InlineData("/file.mytxt", "contents of file.mytxt\n", "application/x-mytxt", "mytxt=application/x-mytxt")]
    // Add a mapping with prefix dot.
    [InlineData("/file.mytxt", "contents of file.mytxt\n", "application/x-mytxt", ".mytxt=application/x-mytxt")]
    // Add several mappings.
    [InlineData("/file.mytxt", "contents of file.mytxt\n", "application/x-mytxt", "mytxt=application/x-mytxt", "notmytxt=application/x-notmytxt")]
    // Check that *.js has a default mapping.
    [InlineData("/file.js", "contents_of_file.js\n", "application/javascript")]
    // Override a default mapping.
    [InlineData("/file.js", "contents_of_file.js\n", "application/my-js", "js=application/my-js")]
    // Remove a default mapping.
    [InlineData("/file.js", "contents_of_file.js\n", null, "js=")]
    public async Task ItAppliesMimeMappings(string file, string expectedContents, string expectedMime, params string[] mimeMap)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestAssets", "Mime");
        using var ds = DotNetServe.Start(path, output: _output, mimeMap: mimeMap);
        var resp = await ds.Client.GetWithRetriesAsync(file);
        if (expectedMime == null)
        {
            Assert.Null(resp.Content.Headers.ContentType);
        }
        else
        {
            Assert.Equal(resp.Content.Headers.ContentType.MediaType, expectedMime);
        }
        var respTxt = await resp.Content.ReadAsStringAsync();
        Assert.Equal(respTxt, expectedContents);
    }
}
