using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Metadata
{
    public class Metadata : IPluginInfo, ILocalizationPlugin, IApiPlugin
    {
        public string Name => "SLThree";
        public bool Insert => true;
        public KeyValuePair<string, Type>[] NewTypes => SLTHelpers.GetTypesFromAssemblySYS(typeof(Metadata).Assembly).ToArray();
        public IDictionary<string, IDictionary<string, string>> Strings => SLTHelpers.GetLocalesFromAssembly(typeof(Metadata).Assembly);
    }
}
