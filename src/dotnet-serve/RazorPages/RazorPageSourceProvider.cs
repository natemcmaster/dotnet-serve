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
        private static Action<ILogger, string, Exception> _readingCsharpFile
            = LoggerMessage.Define<string>(LogLevel.Debug, 0, "Parsing {file} as C#");

        private static Action<ILogger, string, Exception> _csharpFileChanged
            = LoggerMessage.Define<string>(LogLevel.Information, 0, "C# file changed: {file}");
        private static Action<ILogger, string, int, int, string, string, Exception> _csharpSyntaxError
            = LoggerMessage.Define<string, int, int, string, string>(LogLevel.Error, 0, "{file}({line},{col}): error {code}: {message}");

        public RazorPageSourceProvider(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("RazorPages");
            _sourceRoot = env.ContentRootPath;
            _parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            _syntaxTrees = new ConcurrentDictionary<string, SyntaxTree>(StringComparer.Ordinal);
            _fileWatcher = new FileSystemWatcher(_sourceRoot, "*.cs");
            ConfigureCompilerWatcher();
            Initialize();
        }

        public void Initialize()
        {
            foreach (var file in Directory.EnumerateFiles(_sourceRoot, "*.cs", SearchOption.AllDirectories))
            {
                ParseFile(file);
            }
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
            _readingCsharpFile(_logger, path, null);

            var source = SourceText.From(File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, _parseOptions, path);

            foreach (var diag in syntaxTree.GetDiagnostics())
            {
                if (diag.Severity == DiagnosticSeverity.Error)
                {
                    var pos = diag.Location.GetMappedLineSpan().StartLinePosition;
                    _csharpSyntaxError(_logger, path, pos.Line, pos.Character, diag.Id, diag.GetMessage(), null);
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
                Console.WriteLine("Detected file deletion " + e.FullPath);
            };
            _fileWatcher.Renamed += (o, e) =>
            {
                _syntaxTrees.TryRemove(e.OldFullPath, out _);
                Console.WriteLine($"Detected rename {e.OldFullPath} => {e.FullPath}");
                ParseFile(e.FullPath);
            };
            _fileWatcher.Created += (o, e) =>
            {
                Console.WriteLine("Detected new .cs file in " + e.FullPath);
                ParseFile(e.FullPath);
            };
            _fileWatcher.Changed += (o, e) =>
            {
                _csharpFileChanged(_logger, e.FullPath, null);
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
