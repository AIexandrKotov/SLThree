using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class StaticExpression : BaseExpression
    {
        public BaseExpression Right;

        public StaticExpression(object obj) : base(new SourceContext())
        {
            artificial = true;
            this.obj = obj;
            done = true;
        }
        public StaticExpression(BaseExpression right, SourceContext context) : base(context)
        {
            Right = right;
        }

        public override string ExpressionToString() => $"static {Right ?? obj}";

        private bool done;
        private object obj;
        private bool artificial;
        
        public bool IsArtificial => artificial;
        public object Object => obj;

        public override object GetValue(ExecutionContext context)
        {
            if (done) return obj;
            done = true;
            return obj = Right.GetValue(context);
        }

        public override object Clone()
        {
            return artificial ? new StaticExpression(obj) : new StaticExpression(Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
