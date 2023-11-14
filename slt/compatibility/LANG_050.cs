using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace slt
{
    public static class LANG_050
    {
        public static bool Supports { get; internal set; }
        private static FieldInfo TypeofLexem_RegistredAssemblies;
        internal static void Init()
        {
            TypeofLexem_RegistredAssemblies = Program.SLThreeAssembly.GetType("SLThree.TypeofLexem").GetField("RegistredAssemblies");
            TypeofLexem_RegistredAssemblies.GetValue(null).Cast<List<Assembly>>().Add(typeof(LANG_050).Assembly);
        }
    }
}
