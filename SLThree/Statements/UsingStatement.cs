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
        public static Dictionary<string, Type> SystemTypes { get; } = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<")).ToDictionary(x => x.Name, x => x);

        public NameExpression Alias;
        public CreatorUsing Using;

        public UsingStatement(NameExpression alias, CreatorUsing usingBody, SourceContext context) : base(context)
        {
            Alias = alias;
            Using = usingBody;
        }

        public UsingStatement(CreatorUsing @using, SourceContext context) : this(null, @using, context) { }

        public override string ToString() => $"using {Using.Type} as {Alias}";

        public override object GetValue(ExecutionContext context)
        {
            var @using = Using.GetValue(context).Cast<MemberAccess.ClassAccess>();
            if (Alias == null)
            {
                var type_name = @using.Name.GetTypeString();
                var name = type_name.Contains(".") ? @using.Name.Name : type_name;
                context.LocalVariables.SetValue(name, @using);
            }
            else
            {
                var name = Alias.Name;
                context.LocalVariables.SetValue(name, @using);
            }
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement(Using.CloneCast(), SourceContext.CloneCast());
        }
    }
}
