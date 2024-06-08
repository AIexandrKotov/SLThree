using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Expressions.Creators
{
    public class CreatorInstance : BaseExpression
    {
        /*
        ---Creating instances---
        new T;
        new T {};
        new T(args);
        new T(args) {};
        new T: TBase;
        new T: TBase {}; 
        new T(args): TBase;
        new T(args): TBase {};
        ---With name assignation
        new T Name;
        new T Name {};
        new T(args) Name;
        new T(args) Name {};
        new T Name: TBase;
        new T Name: TBase {}; 
        new T(args) Name: TBase;
        new T(args) Name: TBase {};
        */
        public CreatorInstance(TypenameExpression type, BaseExpression[] args, NameExpression name, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context) : base(context)
        {
            Type = type;
            Arguments = args;
            Name = name;
            Ancestors = ancestors;
            CreatorBody = body;
        }
        public CreatorInstance(TypenameExpression type, SourceContext context)
            : this(type, new BaseExpression[0], null, new BaseExpression[0], null, context) { }


        public TypenameExpression Type { get; set; }
        public BaseExpression[] Arguments { get; set; }
        public NameExpression Name { get; set; }
        public BaseExpression[] Ancestors { get; set; }
        public CreatorContextBody CreatorBody { get; set; }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();
            sb.Append("new ");
            sb.Append(Type.ToString());
            if (Arguments.Length > 0)
                sb.Append($"({Arguments.JoinIntoString(", ")})");
            if (Name != null)
                sb.Append($" {Name}");
            if (Ancestors.Length > 0)
            {
                sb.Append(": ");
                sb.Append(Ancestors.JoinIntoString(", "));
            }
            if (CreatorBody != null)
            {
                sb.Append($"{{{CreatorBody}}}");
            }
            return sb.ToString();
        }

        public override object GetValue(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new CreatorInstance(Type.CloneCast(), Arguments.CloneArray(), Name.CloneCast(), Ancestors.CloneArray(), CreatorBody.CloneCast(), SourceContext.CloneCast());
        }
    }

    public class CreatorContextBody : StatementList, ICloneable
    {
        public CreatorContextBody() : base() { }
        public CreatorContextBody(IList<BaseStatement> statements, SourceContext context) : base(statements, context) { }

        public ExecutionContext GetValue(ExecutionContext target, ExecutionContext call, ExecutionContext[] ancestors)
        {
            return target;
        }

        public override object GetValue(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new CreatorContextBody(Statements.CloneArray(), SourceContext.CloneCast());
        }
    }
}
