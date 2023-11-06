using Pegasus.Common;
using SLThree.Extensions;
using System.Linq;

namespace SLThree
{
    public class CreatorArray : BaseLexem
    {
        public BaseLexem[] Lexems;

        public CreatorArray(BaseLexem[] lexems, Cursor cursor) : base(cursor)
        {
            Lexems = lexems;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Lexems.ConvertAll(x => x.GetValue(context)).ToList();
        }

        public override string ToString() => $"[{Lexems.JoinIntoString(", ")}]";
    }
}
