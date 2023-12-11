using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class TypeofExpression : BaseExpression
    {
        public static readonly List<Assembly> RegistredAssemblies = new List<Assembly>()
        {
            typeof(object).Assembly,
            typeof(TypeofExpression).Assembly
        };

        public BaseExpression Typename;
        public TypeofExpression[] GenericArguments;

        public TypeofExpression() : base() { }
        public TypeofExpression(BaseExpression type, SourceContext context) : base(context)
        {
            Typename = type;
            if (this.type == null) this.type = Typename.ExpressionToString().Replace(" ", "").ToType();
            if (this.type == null) throw new LogicalError($"Type \"{Typename}\" not found", context);
        }
        public TypeofExpression(BaseExpression type, TypeofExpression[] generics, SourceContext context) : base(context)
        {
            Typename = type;
            if (this.type == null) this.type = (Typename.ExpressionToString().Replace(" ", "") + "`" + generics.Length.ToString() ).ToType();
            is_generic = true;
            GenericArguments = generics;
            generic_types = GenericArguments.ConvertAll(x => x.type);
        }

        public override string ExpressionToString() => is_generic ? $"typeof({Typename}<{generic_types.JoinIntoString(", ")}>)" : $"typeof({Typename})";

        private Type type;
        private bool is_generic;
        private Type[] generic_types;
        public Type GetTypeofType() => is_generic ? type.MakeGenericType(generic_types) : type;

        public override object GetValue(ExecutionContext context)
        {
            return is_generic ? type.MakeGenericType(generic_types) : type;
        }

        public override object Clone()
        {
            return is_generic 
                ? new TypeofExpression()
                {
                    Typename = Typename.CloneCast(),
                    generic_types = generic_types.ReferenceCopy(),
                    SourceContext = SourceContext.CloneCast(),
                    type = type
                } 
                : new TypeofExpression()
                {
                    Typename = Typename.CloneCast(),
                    SourceContext = SourceContext.CloneCast(),
                    type = type
                };
        }
    }
}
