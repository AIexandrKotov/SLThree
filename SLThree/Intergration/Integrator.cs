using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree.Intergration
{
    public class Integrator
    {
        public interface ILogger
        {
            void Log(int level, string message);
        }
        internal interface ICompilationUnit
        {
            BaseStatement Compile();
        }
        internal class FileUnit : ICompilationUnit
        {
            public string FileName { get; set; }

            public FileUnit(string fileName)
            {
                FileName = fileName;
            }

            public BaseStatement Compile()
            {
                throw new NotImplementedException();
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

            public BaseStatement Compile()
            {
                throw new NotImplementedException();
            }
        }

        public class EnvironmentBuilder
        {
            private ILogger _logger;
            public EnvironmentBuilder(ILogger logger)
            {
                _logger = logger;
                _logger = _logger ?? throw new ArgumentNullException(nameof(logger));
            }

            private List<ICompilationUnit> units;

            public EnvironmentBuilder WithFile(string fileName)
            {
                units.Add(new FileUnit(fileName));
                return this;
            }
            public EnvironmentBuilder WithFile(params string[] fileNames)
            {
                foreach (var fileName in fileNames)
                    WithFile(fileName);
                return this;
            }
            public EnvironmentBuilder WithResource(string resourceName)
            {
                units.Add(new ResourceUnit(Assembly.GetEntryAssembly(), resourceName));
                return this;
            }
            public EnvironmentBuilder WithResources(params string[] resourceNames)
            {
                foreach (var resourceName in resourceNames)
                    WithResources(resourceName);
                return this;
            }
            public EnvironmentBuilder WithResource(Assembly assembly, string resourceName)
            {
                units.Add(new ResourceUnit(assembly, resourceName));
                return this;
            }
            public EnvironmentBuilder WithResources(Assembly assembly, params string[] resourceNames)
            {
                foreach (var resourceName in resourceNames)
                    WithResources(assembly, resourceName);
                return this;
            }

            public void Build()
            {
                var statements = units.Select(x => x.Compile()).ToArray();
            }
        }
    }
}
