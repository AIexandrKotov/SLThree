using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class UsingStatement : BaseStatement
    {
        public BaseLexem Lexem;
        public string Name;

        public static Dictionary<string, Type> SystemTypes { get; } = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<")).ToDictionary(x => x.Name, x => x);

        public UsingStatement(BaseLexem lexem, string name, SourceContext context) : base(context)
        {
            Lexem = lexem;
            Name = name;

            var str = Lexem.ToString().Replace(" ", "");
            if (SystemTypes.ContainsKey(str))
            {
                any_type = new MemberAccess.ClassAccess(SystemTypes[str]);
            }
            else any_type = new MemberAccess.ClassAccess(str.ToType());
        }
        public UsingStatement(BaseLexem lexem, SourceContext context) : base(context)
        {
            Lexem = lexem;
            Name = Lexem.ToString().Split('.').Last().Replace(" ", "");

            var str = Lexem.ToString().Replace(" ", "");
            if (SystemTypes.ContainsKey(str))
            {
                any_type = new MemberAccess.ClassAccess(SystemTypes[str]);
            }
            else any_type = new MemberAccess.ClassAccess(str.ToType());
        }

        private MemberAccess.ClassAccess any_type;

        public override string ToString() => $"using {Lexem} as {Name}";

        public override object GetValue(ExecutionContext context)
        {
            if (any_type.Name == null) throw new RuntimeError($"Type {Lexem.ToString()} not found", SourceContext);
            context.LocalVariables.SetValue(Name, any_type);
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement(Lexem.CloneCast(), Name.CloneCast(), SourceContext.CloneCast());
        }
    }
}
