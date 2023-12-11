using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace slt
{
    public static class LANG_060
    {
        public static bool Supports { get; internal set; }
        private static FieldInfo TypeofExpression_RegistredAssemblies;
        internal static void Init()
        {
            TypeofExpression_RegistredAssemblies = Program.SLThreeAssembly.GetType("SLThree.TypeofExpression").GetField("RegistredAssemblies");
            TypeofExpression_RegistredAssemblies.GetValue(null).Cast<List<Assembly>>().Add(typeof(LANG_060).Assembly);
        }
    }
}
