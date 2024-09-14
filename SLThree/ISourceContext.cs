using System;

namespace SLThree
{
    public abstract class ISourceContext : ICloneable
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Filename { get; set; }

        public override string ToString() => $"{Line}:{Column}{(string.IsNullOrEmpty(Filename) ? "" : $" in {Filename}")}";

        public string ToStringWithoutFile() => $"{Line}:{Column}";

        public abstract object Clone();
    }
}
