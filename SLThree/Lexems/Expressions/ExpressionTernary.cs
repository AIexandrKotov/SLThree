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
            var cond = Condition.GetValue(context);
            var left = Left.GetValue(context);
            var right = Right.GetValue(context);
            return (bool)cond ? left : right;
        }

        public override string ToString() => $"{Condition} ? {Left} : {Right}";
    }
}
