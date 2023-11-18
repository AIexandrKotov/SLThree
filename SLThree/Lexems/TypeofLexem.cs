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
    public class TypeofLexem : BaseLexem
    {
        public static readonly List<Assembly> RegistredAssemblies = new List<Assembly>()
        {
            typeof(object).Assembly,
            typeof(TypeofLexem).Assembly
        };

        public BaseLexem Typename;
        public TypeofLexem[] GenericArguments;

        public TypeofLexem() : base() { }
        public TypeofLexem(BaseLexem type, SourceContext context) : base(context)
        {
            Typename = type;
            if (this.type == null) this.type = Typename.LexemToString().Replace(" ", "").ToType();
            if (this.type == null) throw new LogicalError($"Type \"{Typename}\" not found", context);
        }
        public TypeofLexem(BaseLexem type, TypeofLexem[] generics, SourceContext context) : base(context)
        {
            Typename = type;
            if (this.type == null) this.type = (Typename.LexemToString().Replace(" ", "") + "`" + generics.Length.ToString() ).ToType();
            is_generic = true;
            GenericArguments = generics;
            generic_types = GenericArguments.ConvertAll(x => x.type);
        }

        public override string LexemToString() => is_generic ? $"typeof({Typename}<{generic_types.JoinIntoString(", ")}>)" : $"typeof({Typename})";

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
                ? new TypeofLexem()
                {
                    Typename = Typename.CloneCast(),
                    generic_types = generic_types.ReferenceCopy(),
                    SourceContext = SourceContext.CloneCast(),
                    type = type
                } 
                : new TypeofLexem()
                {
                    Typename = Typename.CloneCast(),
                    SourceContext = SourceContext.CloneCast(),
                    type = type
                };
        }
    }
}
