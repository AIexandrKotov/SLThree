using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class StaticExpression : BaseExpression
    {
        public BaseExpression Right;

        public StaticExpression(BaseExpression right, SourceContext context) : base(context)
        {
            Right = right;
        }

        public override string ExpressionToString() => $"static {Right}";

        private bool done;
        private object obj;

        public override object GetValue(ExecutionContext context)
        {
            if (done) return obj;
            done = true;
            return obj = Right.GetValue(context);
        }

        public override object Clone()
        {
            return new StaticExpression(Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
