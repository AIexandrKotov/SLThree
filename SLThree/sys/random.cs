using SLThree.Extensions;
using System;
using System.Collections;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public class random
    {
        public static object choose(IChooser chooser) => SLTHelpers.random.choose(chooser);
        public static object to_chooser(object o) => SLTHelpers.random.to_chooser(o);
        public static object to_chooser(object o, Type type) => SLTHelpers.random.to_chooser(o, type);
    }
#pragma warning restore IDE1006 // Стили именования
}
