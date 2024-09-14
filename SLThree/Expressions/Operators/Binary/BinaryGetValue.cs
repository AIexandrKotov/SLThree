using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryGetValue : BinaryOperator
    {
        public BinaryGetValue(BaseExpression left, BaseExpression right, ISourceContext context, bool priorityRaised = false) : base(left, right, context, priorityRaised)
        {

        }

        public override object GetValue(ExecutionContext context)
        {
            //return Right.GetValue(context).Cast<ExecutionContext.IExecutable>().GetValue(((ContextWrap)Left.GetValue(context)).Context);
            return Right.GetValue(((ContextWrap)Left.GetValue(context)).Context);
        }

        public override object Clone() => new BinaryGetValue(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);

        public override string Operator => "->";
    }
    public class BinaryGetRuntimeValue : BinaryOperator
    {
        public BinaryGetRuntimeValue(BaseExpression left, BaseExpression right, ISourceContext context, bool priorityRaised = false) : base(left, right, context, priorityRaised)
        {

        }

        public override object GetValue(ExecutionContext context)
        {
            return Right.GetValue(context).Cast<ExecutionContext.IExecutable>().GetValue(((ContextWrap)Left.GetValue(context)).Context);
        }

        public override object Clone() => new BinaryGetRuntimeValue(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);

        public override string Operator => "-->";
    }
}
