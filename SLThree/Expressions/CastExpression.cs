using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq.Expressions;

namespace SLThree
{
    public class CastExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression Type;
        public bool normal;
        public bool as_is;
        public bool as_chooser;

        public CastExpression(BaseExpression left, TypenameExpression type, SourceContext context) : base(context)
        {
            Left = left;
            Type = type;
            var str = Type.ToString();
            if (str == "is") as_is = true;
            else if (str == "chooser") as_chooser = true;
            normal = !(as_is || as_chooser);
        }

        public override string ExpressionToString() => $"{Left} as {Type}";

        public override object GetValue(ExecutionContext context)
        {
            if (normal) return WrappersTypeSetting.UnwrapCast(Type.GetValue(context).Cast<Type>(), Left.GetValue(context));
            else if (as_is) return Left.DropPriority();
            else
            {
                if (Left is EqualchanceChooseExpression ecc) return ecc.GetChooser(context);
                else if (Left is ChanceChooseExpression cc) return cc.GetChooser(context);
                throw new RuntimeError($"{Left?.GetType().GetTypeString() ?? "null"} is not a chooser", Left.SourceContext);
            }
        }

        public override object Clone()
        {
            return new CastExpression(Left.CloneCast(), Type.CloneCast(), SourceContext.CloneCast());
        }
    }
}
