using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class NewExpression : BaseExpression
    {
        public TypenameExpression Typename;
        public BaseExpression[] Arguments;

        public NewExpression(TypenameExpression typename, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Typename = typename;
            Arguments = arguments;
        }

        public NewExpression(TypenameExpression typename, SourceContext context) : this(typename, new BaseExpression[0], context) { }

        public override string ExpressionToString() => $"new {Typename}({Arguments.JoinIntoString(", ")})";

        public override object GetValue(ExecutionContext context)
        {
            return Activator.CreateInstance(Typename.GetValue(context).Cast<Type>(), Arguments.ConvertAll(x => x.GetValue(context)));
        }

        public override object Clone()
        {
            return new NewExpression(Typename.CloneCast(), Arguments.CloneArray(), SourceContext.CloneCast());
        }
    }
}
