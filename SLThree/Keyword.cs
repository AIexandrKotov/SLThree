using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class Keyword
    {
        public bool IsKeyword(string s)
        {
            switch (s)
            {
                case "while":
                case "if":
                case "else":
                    return true;
            }
            return false;
        }
    }
}
