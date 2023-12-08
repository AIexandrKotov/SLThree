using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq.Expressions;

namespace SLThree
{
    public class CastLexem : ExpressionBinary
    {
        public CastLexem(BaseLexem castingLexem, BaseLexem castingType, SourceContext context) : base(castingLexem, castingType, context)
        {
            name = Right.LexemToString().Replace(" ", "");
            mode = name == "\\" ? 2 : (name == "is" ? 1 : -1);
            if (mode == -1)
            {
                if (type == null) type = name.ToType();
                if (type == null) mode = 0;
            }
        }

        public int mode = 0; // -1 - predefined, 0 - find type, 1 - as is, 2 - as \

        public override string LexemToString() => $"{Left} as {Right}";

        private string name;
        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            if (mode == -1)
            {
                return Left.GetValue(context).CastToType(type);
            }
            if (mode == 0)
            {
                var obj = Right.GetValue(context);
                if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", Right.SourceContext);
                if (obj is Type tp) return Left.GetValue(context).CastToType(tp);
                return Left.GetValue(context).CastToType((obj as MemberAccess.ClassAccess).Name);
            }

            if (mode == 1) return Left.DropPriority();
            if (mode == 2)
            {
                if (Left is EqualchanceChooseLexem ecc) return ecc.GetChooser(context);
                else if (Left is ChanceChooseLexem cc) return cc.GetChooser(context);
                throw new RuntimeError($"{Left?.GetType().GetTypeString() ?? "null"} is not a chooser", Left.SourceContext);
            }
            throw new OperatorError(this, Left?.GetType(), Right?.GetType());
        }

        public override string Operator => "as";

        public override object Clone() => new CastLexem(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
    }
}
