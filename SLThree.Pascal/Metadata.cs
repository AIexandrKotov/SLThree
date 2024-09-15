using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Pascal
{
    public class Metadata : IPluginInfo, ILanguageProvider, IApiPlugin
    {
        public string Name => "Pascal";
        public bool Insert => true;
        public string Edition => "";
        public IParser Parser => new Parser();
        public IRestorator Restorator => new Restorator();
        public KeyValuePair<string, Type>[] NewTypes => SLTHelpers.GetTypesFromAssemblySYS(typeof(Metadata).Assembly, "SLThree.Pascal.sys.").ToArray();
    }
}
