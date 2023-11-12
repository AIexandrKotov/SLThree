using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pegasus.Common;

namespace SLThree
{
    public class SourceContext : ICloneable
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Filename { get; set; }

        public SourceContext() { }
        public SourceContext(Cursor cursor)
        {
            Line = cursor.Line;
            Column = cursor.Column;
            Filename = cursor.FileName;
        }

        public override string ToString() => $"{Line}:{Column}{(string.IsNullOrEmpty(Filename) ? "" : $" in {Filename}")}";

        public object Clone() => new SourceContext() { Line = Line, Column = Column, Filename = Filename };

        public static implicit operator SourceContext(Cursor cursor) => new SourceContext(cursor);
    }
}
