using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public partial class Parser
    {
        public BaseStatement ParseScript(string s, string filename = null) => Parse("#SLT# " + s, filename);
    }
}
