using Microsoft.Win32;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public static class DotnetEnvironment
    {
        public static LanguageInformation.IParser DefaultParser;
        public static readonly List<Assembly> RegistredAssemblies;
        public static readonly Dictionary<string, Type> SystemTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<") && x.IsPublic)
            .ToDictionary(x => x.Name, x => x);

        static DotnetEnvironment()
        {
            RegistredAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
#if NET || NETSTANDARD
            RegistredAssemblies.Add(typeof(Console).Assembly);
#endif
        }
    }
}
