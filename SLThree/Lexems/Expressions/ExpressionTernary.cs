using Pegasus.Common;

namespace SLThree
{
    public class ExpressionTernary : BaseLexem
    {
        public string Operator => "?:";

        public BaseLexem Condition;
        public BaseLexem Left;
        public BaseLexem Right;

        public ExpressionTernary(BaseLexem cond, BaseLexem left, BaseLexem right, Cursor cursor) : base(cursor)
        {
            Condition = cond;
            Left = left;
            Right = right;
        }
        public ExpressionTernary() : base(default) { }
        public override object GetValue(ExecutionContext context)
        {
            object cond = Condition.GetValue(context);
            object left = Left.GetValue(context);
            object right = Right.GetValue(context);
            if (cond is bool b1) return b1 ? left : right;
            throw new OperatorError(Operator, cond?.GetType(), SourceContext);
        }

        public override string ToString() => $"{Condition} ? {Left} : {Right}";
    }
}
