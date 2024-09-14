using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slt
{
    public class Metadata : IPluginInfo, ILocalizationPlugin, IApiPlugin
    {
        public string Name => "SLThree REPL";
        public bool Insert => true;
        public KeyValuePair<string, Type>[] NewTypes => SLTHelpers.GetTypesFromAssemblySYS(typeof(Metadata).Assembly, "slt.sys.").ToArray();
        public IDictionary<string, IDictionary<string, string>> Strings => SLTHelpers.GetLocalesFromAssembly(typeof(Metadata).Assembly, "slt.docs.locales.");
    }
}
