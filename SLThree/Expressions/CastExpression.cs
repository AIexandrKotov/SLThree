using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq.Expressions;

namespace SLThree
{
    public class CastExpression : ExpressionBinary
    {
        public CastExpression(BaseExpression castingExpression, BaseExpression castingType, SourceContext context) : base(castingExpression, castingType, context)
        {
            name = Right.ExpressionToString().Replace(" ", "");
            mode = name == "\\" ? 2 : (name == "is" ? 1 : -1);
            if (mode == -1)
            {
                if (type == null) type = name.ToType();
                if (type == null) mode = 0;
            }
        }

        public int mode = 0; // -1 - predefined, 0 - find type, 1 - as is, 2 - as \

        public override string ExpressionToString() => $"{Left} as {Right}";

        private string name;
        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            if (mode == -1)
            {
                return WrappersTypeSetting.UnwrapCast(type, Left.GetValue(context));
            }
            if (mode == 0)
            {
                var obj = Right.GetValue(context);
                if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", Right.SourceContext);
                if (obj is Type tp) return WrappersTypeSetting.UnwrapCast(tp,  Left.GetValue(context));
                return WrappersTypeSetting.UnwrapCast((obj as MemberAccess.ClassAccess).Name, Left.GetValue(context));
            }

            if (mode == 1) return Left.DropPriority();
            if (mode == 2)
            {
                if (Left is EqualchanceChooseExpression ecc) return ecc.GetChooser(context);
                else if (Left is ChanceChooseExpression cc) return cc.GetChooser(context);
                throw new RuntimeError($"{Left?.GetType().GetTypeString() ?? "null"} is not a chooser", Left.SourceContext);
            }
            throw new OperatorError(this, Left?.GetType(), Right?.GetType());
        }

        public override string Operator => "as";

        public override object Clone() => new CastExpression(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
    }
}
