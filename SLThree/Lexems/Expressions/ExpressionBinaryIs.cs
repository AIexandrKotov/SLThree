using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class ExpressionBinaryIs : ExpressionBinary
    {
        public ExpressionBinaryIs(BaseLexem castingLexem, BaseLexem castingType, SourceContext context, bool priority = false) : base(castingLexem, castingType, context, priority)
        {
            name = Right.LexemToString().Replace(" ", "");
            mode = name == "\\" ? 2 : (name == "is" ? 1 : -1);
            if (mode == -1)
            {
                if (type == null) type = name.ToType();
                if (type == null) mode = 0;
            }
        }

        public int mode = 0; // -1 - predefined, 0 - find type
        public bool variable_assigned = false;
        public int variable_index;

        public override string LexemToString() => $"{Left} is {Right}";

        private string name;
        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            if (mode == -1)
            {
                return Left.GetValue(context).GetType().IsType(type);
            }
            if (mode == 0)
            {
                if (variable_assigned) return Left.GetValue(context).GetType().IsType((context.LocalVariables.GetValue(variable_index) as MemberAccess.ClassAccess).Name);
                var (obj, ind) = context.LocalVariables.GetValue(name);
                variable_index = ind;
                if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", Right.SourceContext);
                return Left.GetValue(context).GetType().IsType((obj as MemberAccess.ClassAccess).Name);
            }
            throw new OperatorError(this, Left?.GetType(), Right?.GetType());
        }

        public override string Operator => "is";

        public override object Clone() => new ExpressionBinaryIs(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
    }
}
