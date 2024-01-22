using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class CreatorUsing : BaseExpression
    {
        public TypenameExpression Type;
        public MemberAccess.ClassAccess system;

        public CreatorUsing(TypenameExpression type, SourceContext context) : base(context)
        {
            Type = type;
            if (type.Generics == null)
            {
                var str = type.Typename.ToString();
                if (UsingStatement.SystemTypes.TryGetValue(str, out var sys))
                    system = new MemberAccess.ClassAccess(sys);
            }
        }

        public override string ExpressionToString() => $"new using {Type}";

        public string GetTypenameWithoutGenerics() => Type.Typename.ToString();

        public override object GetValue(ExecutionContext context)
        {
            if (system != null) return system;
            return new MemberAccess.ClassAccess(Type.GetValue(context).Cast<Type>());
        }

        public override object Clone()
        {
            return new CreatorUsing(Type.CloneCast(), SourceContext.CloneCast());
        }
    }
}