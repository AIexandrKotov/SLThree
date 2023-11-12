using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ExpressionTernary : BaseLexem
    {
        public string Operator => "?:";

        public BaseLexem Condition;
        public BaseLexem Left;
        public BaseLexem Right;
        
        public ExpressionTernary(BaseLexem cond, BaseLexem left, BaseLexem right, SourceContext context) : base(context)
        {
            Condition = cond;
            Left = left;
            Right = right;
        }
        public ExpressionTernary() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var cond = Condition.GetValue(context);
            var left = Left.GetValue(context);
            var right = Right.GetValue(context);
            return (bool)cond ? left : right;
        }

        public override string ToString() => $"{Condition} ? {Left} : {Right}";

        public override object Clone()
        {
            return new ExpressionTernary(Condition.CloneCast(), Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
