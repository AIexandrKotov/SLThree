using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class CastExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression Type;
        public bool normal;
        public bool as_is;

        public CastExpression(BaseExpression left, TypenameExpression type, SourceContext context) : base(context)
        {
            Left = left;
            Type = type;
            var str = Type.ToString();
            if (str == "is") as_is = true;
            normal = !(as_is);
        }

        public override string ExpressionToString() => $"{Left} as {Type}";

        public override object GetValue(ExecutionContext context)
        {
            if (normal) return Wrapper.UnwrapCast(Type.GetValue(context).Cast<Type>(), Left.GetValue(context));
            else return Left.CloneCast().DropPriority();
        }

        public override object Clone()
        {
            return new CastExpression(Left.CloneCast(), Type.CloneCast(), SourceContext.CloneCast());
        }
    }
}
