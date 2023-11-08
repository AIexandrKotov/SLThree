using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Tools.Generic
{
    public interface IChooser<out T> : IChooser, IEnumerable, IEnumerable<T>
    {
        new T Choose();
    }
}
