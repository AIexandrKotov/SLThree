using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class TernaryOperator : BaseExpression
    {
        public string Operator => "?:";

        public BaseExpression Condition;
        public BaseExpression Left;
        public BaseExpression Right;

        public TernaryOperator(BaseExpression cond, BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(priority, context)
        {
            Condition = cond;
            Left = left;
            Right = right;
        }
        public TernaryOperator() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var cond = Condition.GetValue(context);
            return (bool)cond ? Left.GetValue(context) : Right.GetValue(context);
        }

        public override string ExpressionToString() => $"{Condition} ? {Left} : {Right}";

        public override object Clone()
        {
            return new TernaryOperator(Condition.CloneCast(), Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
