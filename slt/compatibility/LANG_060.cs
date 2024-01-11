using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace slt
{
    public static class LANG_060
    {
        public static bool Supports { get; internal set; }
        private static FieldInfo slt_RegistredAssemblies;
        internal static void Init()
        {
            slt_RegistredAssemblies = Program.SLThreeAssembly.GetType("SLThree.sys.slt").GetField("RegistredAssemblies");
            slt_RegistredAssemblies.GetValue(null).Cast<List<Assembly>>().Add(typeof(LANG_060).Assembly);
        }
    }
}
