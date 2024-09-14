using Pegasus.Common;

namespace SLThree.Pascal
{
    public class SourceContext : ISourceContext
    {
        public SourceContext() { }
        public SourceContext(Cursor cursor)
        {
            Line = cursor.Line;
            Column = cursor.Column;
            Filename = cursor.FileName;
        }

        public static implicit operator SourceContext(Cursor cursor) => new SourceContext(cursor);
        public override object Clone() => new SourceContext() { Line = Line, Column = Column, Filename = Filename };
    }
}
