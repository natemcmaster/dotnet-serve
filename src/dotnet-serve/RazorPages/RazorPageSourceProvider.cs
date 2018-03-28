// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Server.RazorPages
{
    class RazorPageSourceProvider : IDisposable
    {
        public ILogger _logger { get; }

        private readonly string _sourceRoot;
        private readonly CSharpParseOptions _parseOptions;
        private ConcurrentDictionary<string, SyntaxTree> _syntaxTrees;
        private readonly FileSystemWatcher _fileWatcher;

        public RazorPageSourceProvider(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("RazorPages");
            _sourceRoot = env.ContentRootPath;
            _parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            _syntaxTrees = new ConcurrentDictionary<string, SyntaxTree>(StringComparer.Ordinal);
            _fileWatcher = new FileSystemWatcher(_sourceRoot, "*.cs");
        }

        public void Initialize()
        {
            foreach (var file in Directory.EnumerateFiles(_sourceRoot, "*.cs", SearchOption.AllDirectories))
            {
                ParseFile(file);
            }
            ConfigureCompilerWatcher();
        }

        public void OnCompilation(RoslynCompilationContext ctx)
        {
            foreach (var syntaxTree in _syntaxTrees.Values)
            {
                ctx.Compilation = ctx.Compilation.AddSyntaxTrees(syntaxTree);
            }
        }

        private void ParseFile(string path)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Parsing {file} as C#", path);
            }

            var source = SourceText.From(File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, _parseOptions, path);

            foreach (var diag in syntaxTree.GetDiagnostics())
            {
                if (diag.Severity == DiagnosticSeverity.Error)
                {
                    var pos = diag.Location.GetMappedLineSpan().StartLinePosition;
                    _logger.LogError("{file}({line},{col}): error {code}: {message}", _logger, path, pos.Line, pos.Character, diag.Id, diag.GetMessage());
                }
            }

            _syntaxTrees.AddOrUpdate(path, syntaxTree, (_, __) => syntaxTree);
        }

        private void ConfigureCompilerWatcher()
        {
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.Deleted += (o, e) =>
            {
                _syntaxTrees.TryRemove(e.FullPath, out _);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Detected file deletion of {path}", e.FullPath);
                }
            };
            _fileWatcher.Renamed += (o, e) =>
            {
                _syntaxTrees.TryRemove(e.OldFullPath, out _);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Detected rename {old} => {new}", e.OldFullPath, e.FullPath);
                }
                ParseFile(e.FullPath);
            };
            _fileWatcher.Created += (o, e) =>
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Detected new .cs file in {path}", e.FullPath);
                }
                ParseFile(e.FullPath);
            };
            _fileWatcher.Changed += (o, e) =>
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("C# file changed: {path}", e.FullPath);
                }
                ParseFile(e.FullPath);
            };
            _fileWatcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}
