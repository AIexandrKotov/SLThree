using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections;

namespace SLThree
{
    public class UnaryChoose : UnaryOperator
    {
        public override string Operator => "^";
        public UnaryChoose(BaseExpression left, ISourceContext context, bool priority = false) : base(left, context, priority) { }
        public UnaryChoose() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);
            return left is IChooser chooser ? chooser.Choose() : SLTHelpers.random.to_chooser(left).Cast<IChooser>().Choose();
        }
        public override object Clone()
        {
            return new UnaryChoose(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
