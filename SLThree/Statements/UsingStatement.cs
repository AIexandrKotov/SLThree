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

        public UsingStatement() { }
        public UsingStatement(BaseLexem lexem, string name, SourceContext context) : base(context)
        {
            var type_name = lexem.ToString().Replace(" ", "");
            if (SystemTypes.ContainsKey(type_name))
            {
                Lexem = lexem;
                Name = type_name;
                any_type = new MemberAccess.ClassAccess(SystemTypes[type_name]);
            }
            else if (lexem is TypeofLexem tl)
            {
                Lexem = tl;
                Name = name.Split('.').Last();
                any_type = new MemberAccess.ClassAccess(tl.GetTypeofType());
            }
            else
            {
                Lexem = new TypeofLexem(lexem, context);
                Name = name.Split('.').Last();
                any_type = new MemberAccess.ClassAccess((Lexem as TypeofLexem).GetTypeofType());
            }
        }
        public UsingStatement(BaseLexem lexem, BaseLexem name, SourceContext context) : this(lexem, name.ToString(), context)
        {

        }
        public UsingStatement(BaseLexem lexem, SourceContext context) : this(lexem, lexem.ToString().Replace(" ", ""), context)
        {

        }

        private MemberAccess.ClassAccess any_type;

        public override string ToString() => $"using {Lexem} as {Name}";

        public override object GetValue(ExecutionContext context)
        {
            if (any_type.Name == null) throw new RuntimeError($"Type {Lexem.LexemToString()} not found", SourceContext);
            context.LocalVariables.SetValue(Name, any_type);
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement()
            {
                Lexem = Lexem.CloneCast(),
                Name = Name.CloneCast(),
                SourceContext = SourceContext.CloneCast(),
                any_type = any_type
            };
        }
    }
}
