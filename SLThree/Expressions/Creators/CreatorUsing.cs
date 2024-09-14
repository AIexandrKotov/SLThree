using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class CreatorUsing : BaseExpression
    {
        public TypenameExpression Type;
        public ClassAccess system;

        public CreatorUsing(TypenameExpression type, ISourceContext context) : base(context)
        {
            Type = type;
            if (type.Generics == null)
            {
                var str = type.Typename.ToString();
                if (DotnetEnvironment.SystemTypes.TryGetValue(str, out var sys))
                    system = new ClassAccess(sys);
            }
        }

        public override string ExpressionToString() => $"new using {Type}";

        public string GetTypenameWithoutGenerics() => Type.Typename.ToString();

        public override object GetValue(ExecutionContext context)
        {
            if (system != null) return system;
            return new ClassAccess(Type.GetValue(context).Cast<Type>());
        }

        public override object Clone()
        {
            return new CreatorUsing(Type.CloneCast(), SourceContext.CloneCast());
        }
    }
}