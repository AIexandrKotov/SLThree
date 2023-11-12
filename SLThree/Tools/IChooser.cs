using System.Collections.Generic;

namespace System.Collections
{
    public interface IChooser : IEnumerable
    {
        object Choose();
    }

    public interface IChanceChooser : IChooser
    {
        IList<(object, double)> Values { get; }
    }

    public interface IEqualchanceChooser : IChooser
    {
        IList<object> Values { get; }
    }
}