using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryAssignMacro : BinaryOperator
    {
        public BinaryAssignMacro(BaseExpression left, BaseExpression right, SourceContext context) : base(left, right, context)
        {
        }

        public override string Operator => "<-";



        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context).Cast<Macros>().set(Right.GetValue(context));
        }

        public override object Clone()
        {
            return new BinaryAssignMacro(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
