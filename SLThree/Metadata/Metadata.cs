using SLThree.Extensions;
using SLThree.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Metadata
{
    public class Metadata : IPluginInfo, IDescription, ILocalizationPlugin, ILanguageProvider, IApiPlugin
    {
        public string Name => "SLThree";
        public bool Insert => true;
        public KeyValuePair<string, Type>[] NewTypes => SLTHelpers.GetTypesFromAssemblySYS(typeof(Metadata).Assembly).ToArray();
        public IDictionary<string, IDictionary<string, string>> Strings => SLTHelpers.GetLocalesFromAssembly(typeof(Metadata).Assembly);
        public string Edition => "Massive Update";
        public IParser Parser => new Parser();
        public IRestorator Restorator => new Restorator();
        public string Description => "";
        public string ChangeLog => SLTHelpers.GetStringFromAssembly(typeof(Metadata).Assembly, "SLThree.docs.versions.last");
    }
}
