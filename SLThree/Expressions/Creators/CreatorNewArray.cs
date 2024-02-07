using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class CreatorNewArray : BaseExpression
    {
        public TypenameExpression ArrayType;
        public BaseExpression Size;

        public CreatorNewArray(TypenameExpression arrayType, BaseExpression size, SourceContext context) : base(context)
        {
            ArrayType = arrayType;
            Size = size;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Array.CreateInstance(ArrayType.GetValue(context).Cast<Type>(), Size.GetValue(context).CastToType(typeof(int)).Cast<int>());
        }

        public override string ExpressionToString() => $"new {ArrayType}[{Size}]";

        public override object Clone()
        {
            return new CreatorNewArray(ArrayType.CloneCast(), Size.CloneCast(), SourceContext.CloneCast());
        }
    }
}
