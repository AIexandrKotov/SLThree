using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq.Expressions;
using System.Xml;

namespace SLThree
{
    public class CastLexem : ExpressionBinary
    {
        public CastLexem(BaseLexem castingLexem, BaseLexem castingType, SourceContext context) : base(castingLexem, castingType, context)
        {
            name = Right.ToString().Replace(" ", "");
            mode = name == "\\" ? 2 : (name == "is" ? 1 : -1);
            if (mode == -1)
            {
                if (type == null) type = name.ToType();
                if (type == null) mode = 0;
            }
        }
        public CastLexem(BaseLexem castingLexem, BaseLexem castingType, Cursor cursor)
            : this(castingLexem, castingType, new SourceContext(cursor)) { }

        public int mode = 0; // -1 - predefined, 0 - find type, 1 - as is, 2 - as \
        public bool variable_assigned = false;
        public int variable_index;

        public override string ToString() => $"{Left} as {Right}";

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
                if (variable_assigned) return Left.GetValue(context).CastToType((context.LocalVariables.GetValue(variable_index) as MemberAccess.ClassAccess).Name);
                var (obj, ind) = context.LocalVariables.GetValue(name);
                variable_index = ind;
                if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", Right.SourceContext);
                return Left.GetValue(context).CastToType((obj as MemberAccess.ClassAccess).Name);
            }

            if (mode == 1) return Left;
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
