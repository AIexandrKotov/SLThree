using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class SLThreeInformation : ILanguageInformation
    {
        public string Name => "SLThree";
        public string Version => default;
        public string Edition => "Massive Update";
        public LanguageInformation.IParser Parser => new Parser();
        public LanguageInformation.IRestorator Restorator => throw new NotImplementedException();
    }
}
