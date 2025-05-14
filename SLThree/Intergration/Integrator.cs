using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SLThree.Intergration
{
    public class Integrator
    {
        internal interface ICompilationUnit
        {
            BaseStatement Compile(IParser parser);
        }

        internal class FileUnit : ICompilationUnit
        {
            public string FileName { get; set; }

            public FileUnit(string fileName)
            {
                FileName = fileName;
            }

            public BaseStatement Compile(IParser parser)
            {
                return parser.ParseScript(File.ReadAllText(FileName), FileName);
            }
        }
        internal class ResourceUnit : ICompilationUnit
        {
            public Assembly Assembly { get; set; }
            public string ResourceName { get; set; }

            public ResourceUnit(Assembly assembly, string resourceName)
            {
                Assembly = assembly;
                ResourceName = resourceName;
            }

            public BaseStatement Compile(IParser parser)
            {
                return parser.ParseScript(Assembly.GetManifestResourceStream(ResourceName).ReadString(), ResourceName);
            }
        }
        internal class CodeUnit : ICompilationUnit
        {
            public string Code { get; set; }
            public string ScriptName { get; set; }
            public CodeUnit(string code, string scriptName)
            {
                Code = code;
                ScriptName = scriptName;
            }
            public BaseStatement Compile(IParser parser)
            {
                return parser.ParseScript(Code, ScriptName ?? "script");
            }
        }
        internal class AlreadyCompiledUnit : ICompilationUnit
        {
            public BaseStatement Compiled { get; set; }

            public AlreadyCompiledUnit(BaseStatement compiled)
            {
                Compiled = compiled;
            }

            public BaseStatement Compile(IParser parser)
            {
                return Compiled;
            }
        }
        internal class NestedScriptBuilderUnit : ICompilationUnit
        {
            public ScriptBuilder ScriptBuilder { get; set; }
            public IParser Parser { get; set; }

            public NestedScriptBuilderUnit(ScriptBuilder scriptBuilder, IParser parser)
            {
                ScriptBuilder = scriptBuilder;
                Parser = parser;
            }

            public BaseStatement Compile(IParser parser)
            {
                var stst = ScriptBuilder
                    .Compile(Parser ?? parser)
                    .CompiledStatements;

                if (stst.Length < 1)
                    return new EmptyStatement(new SourceContext());

                var ctx = stst[0].SourceContext;

                return new StatementList(stst, ctx);
            }
        }

        public class RunOptions
        {
            public bool AlwaysPrepareContext { get; set; } = true;
        }
        public class BuilderOptions
        {
            public bool ThrowIfError { get; set; } = true;
            public bool UseLogging { get; set; } = false;
            public Action<string> Logger { get; set; } = x => Console.WriteLine(x);
            public Action<Exception> ExceptionHandler { get; set; }

            public BuilderOptions()
            {
                ExceptionHandler = x =>
                {
                    if (UseLogging)
                        Logger(x.ToString());
                };
            }
        }

        public class ScriptBuilder
        {
            public BuilderOptions Options { get; set; }
            public bool Success { get; set; } = false;
            public bool Compiled => CompiledStatements != null;
            public BaseStatement[] CompiledStatements { get; set; } = null;

            public ScriptBuilder(Action<BuilderOptions> config)
            {
                Options = new BuilderOptions();
                config(Options);
                units = new List<ICompilationUnit>();
            }
            public ScriptBuilder() : this(cfg => { }) { }

            private List<ICompilationUnit> units;

            public ScriptBuilder WithCode(string code, string scriptname = null)
            {
                units.Add(new CodeUnit(code, scriptname ?? Guid.NewGuid().ToString()));
                return this;
            }
            public ScriptBuilder WithFile(string fileName)
            {
                units.Add(new FileUnit(fileName));
                return this;
            }
            public ScriptBuilder WithFiles(params string[] fileNames)
            {
                foreach (var fileName in fileNames)
                    WithFile(fileName);
                return this;
            }

            public ScriptBuilder WithDirectory(string directory, bool only_slt = true, bool onlytop = false)
            {
                var fileNames = Directory.GetFiles(directory, only_slt ? "*.slt" : "*", onlytop ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
                foreach (var fileName in fileNames)
                    WithFile(fileName);
                return this;
            }

            public ScriptBuilder WithDirectories(string[] directories, bool only_slt = true, bool onlytop = false)
            {
                foreach (var directoryName in directories)
                    WithDirectory(directoryName, only_slt, onlytop);
                return this;
            }

            public ScriptBuilder WithDirectories(params string[] directories)
            {
                foreach (var directoryName in directories)
                    WithDirectory(directoryName);
                return this;
            }

            public ScriptBuilder WithResource(string resourceName)
            {
                units.Add(new ResourceUnit(Assembly.GetEntryAssembly(), resourceName));
                return this;
            }
            public ScriptBuilder WithResources(params string[] resourceNames)
            {
                foreach (var resourceName in resourceNames)
                    WithResources(resourceName);
                return this;
            }
            public ScriptBuilder WithResource(Assembly assembly, string resourceName)
            {
                units.Add(new ResourceUnit(assembly, resourceName));
                return this;
            }
            public ScriptBuilder WithResources(Assembly assembly, params string[] resourceNames)
            {
                foreach (var resourceName in resourceNames)
                    WithResources(assembly, resourceName);
                return this;
            }
            
            public ScriptBuilder AddBuilder(ScriptBuilder scriptBuilder, IParser parser = null)
            {
                units.Add(new NestedScriptBuilderUnit(scriptBuilder, parser));
                return this;
            }
            public ScriptBuilder AddCompiled(BaseStatement statement)
            {
                units.Add(new AlreadyCompiledUnit(statement));
                return this;
            }

            public ScriptBuilder Compile(IParser parser)
            {
                var statements = new List<BaseStatement>();
                Success = true;
                foreach (var unit in units)
                {
                    try
                    {
                        statements.Add(unit.Compile(parser));
                    }
                    catch (Exception ex)
                    {
                        Options.ExceptionHandler(ex);
                        Success = false;
                        if (Options.ThrowIfError)
                            throw;
                    }
                }
                CompiledStatements = statements.ToArray();
                return this;
            }

            public ExecutionContext Run(RunOptions options = null, ExecutionContext context = null)
            {
                options = options ?? new RunOptions();
                context = context ?? new ExecutionContext();

                foreach (var compiled in CompiledStatements)
                {
                    if (options.AlwaysPrepareContext)
                        context.PrepareToInvoke();
                    compiled.GetValue(context);
                }
                return context;
            }
        }
    }
}
