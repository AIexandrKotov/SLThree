using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    //Typed linq with delegates
    public class dlinq
    {
        //todo make all methods
        public static TOut max<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> func)
        {
            return objects.Max(x => func(x));
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
