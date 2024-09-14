using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree.Language
{
    [SLThreeLanguage]
    public class Metadata : IPluginInfo, ILanguageProvider, IApiPlugin
    {
        public string Name => "SLThree.Language";

        public bool Insert => true;

        public string Edition => "Massive Update";

        public IParser Parser => new Parser();

        public IRestorator Restorator => new Restorator();

        public KeyValuePair<string, Type>[] NewTypes => SLTHelpers.GetTypesFromAssemblySYS(typeof(Metadata).Assembly, "SLThree.Language.sys.").ToArray();
    }
}
