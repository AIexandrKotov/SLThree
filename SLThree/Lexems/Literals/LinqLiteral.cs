using Pegasus.Common;

namespace SLThree
{
    public class LinqLiteral : BaseLexem
    {
        public LinqLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "linq";

        public override object GetValue(ExecutionContext context) => Linq.Linq.LinqAccess;
    }
}
