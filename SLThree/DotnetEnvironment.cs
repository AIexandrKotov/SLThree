using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public static class DotnetEnvironment
    {
        public static IParser DefaultParser;
        public static readonly List<Assembly> RegistredAssemblies;
        public static readonly IDictionary<string, Type> SystemTypes = new ConcurrentDictionary<string, Type>(SLTHelpers.GetTypesFromAssemblySYS(typeof(DotnetEnvironment).Assembly));

        static DotnetEnvironment()
        {
            RegistredAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
#if NET || NETSTANDARD
            RegistredAssemblies.Add(typeof(Console).Assembly);
#endif
        }
    }
}
