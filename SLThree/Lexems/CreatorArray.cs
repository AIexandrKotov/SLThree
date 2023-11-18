using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class CreatorArray : BaseLexem
    {
        public BaseLexem[] Lexems;

        public CreatorArray(BaseLexem[] lexems, SourceContext context) : base(context)
        {
            Lexems = lexems;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Lexems.ConvertAll(x => x.GetValue(context)).ToList();
        }

        public override string LexemToString() => $"[{Lexems.JoinIntoString(", ")}]";

        public override object Clone()
        {
            return new CreatorArray(Lexems.CloneArray(), SourceContext);
        }
    }
}
