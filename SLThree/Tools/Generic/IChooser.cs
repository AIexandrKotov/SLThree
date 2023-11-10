namespace System.Collections.Generic
{
    public interface IChooser<out T> : IChooser, IEnumerable, IEnumerable<T>
    {
        new T Choose();
    }

    public interface IChanceChooser<T> : IChooser<T>, IChanceChooser
    {
        new IList<(T, double)> Values { get; }
    }

    public interface IEqualchanceChooser<T> : IChooser<T>, IEqualchanceChooser
    {
        new IList<T> Values { get; }
    }
}
