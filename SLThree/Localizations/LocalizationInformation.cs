using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Localizations
{
    public class LocalizationInformation : ILocalizedAssemblyInformation
    {
        public string Name { get; private set; }

        public string Version { get; private set; }
    }
}
