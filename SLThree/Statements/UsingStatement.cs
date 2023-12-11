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
        public BaseExpression Expression;
        public string Name;

        public static Dictionary<string, Type> SystemTypes { get; } = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<")).ToDictionary(x => x.Name, x => x);

        public UsingStatement() { }
        public UsingStatement(BaseExpression expression, string name, SourceContext context) : base(context)
        {
            var type_name = expression.ToString().Replace(" ", "");
            if (SystemTypes.ContainsKey(type_name))
            {
                Expression = expression;
                Name = type_name;
                any_type = new MemberAccess.ClassAccess(SystemTypes[type_name]);
            }
            else if (expression is TypeofExpression tl)
            {
                Expression = tl;
                Name = name.Split('.').Last();
                any_type = new MemberAccess.ClassAccess(tl.GetTypeofType());
            }
            else
            {
                Expression = new TypeofExpression(expression, context);
                Name = name.Split('.').Last();
                any_type = new MemberAccess.ClassAccess((Expression as TypeofExpression).GetTypeofType());
            }
        }
        public UsingStatement(BaseExpression expression, BaseExpression name, SourceContext context) : this(expression, name.ToString(), context)
        {

        }
        public UsingStatement(BaseExpression expression, SourceContext context) : this(expression, expression.ToString().Replace(" ", ""), context)
        {

        }

        private MemberAccess.ClassAccess any_type;

        public override string ToString() => $"using {Expression} as {Name}";

        public override object GetValue(ExecutionContext context)
        {
            if (any_type.Name == null) throw new RuntimeError($"Type {Expression.ExpressionToString()} not found", SourceContext);
            context.LocalVariables.SetValue(Name, any_type);
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement()
            {
                Expression = Expression.CloneCast(),
                Name = Name.CloneCast(),
                SourceContext = SourceContext.CloneCast(),
                any_type = any_type
            };
        }
    }
}
