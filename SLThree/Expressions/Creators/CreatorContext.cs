using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class CreatorContext : BaseExpression
    {
        public NameExpression Name;
        public TypenameExpression Typecast;
        public BaseStatement[] Body;

        public bool HasCast => Typecast != null;
        public bool HasName => Name != null;
        public bool HasBody => Body.Length > 0;

        public CreatorContext(SourceContext context) : this(null, null, new BaseStatement[0], context) { }

        public CreatorContext(NameExpression name, SourceContext context) : this(name, null, new BaseStatement[0], context) { }

        public CreatorContext(NameExpression name, TypenameExpression typecast, SourceContext context) : this(name, typecast, new BaseStatement[0], context) { }

        public CreatorContext(BaseStatement[] body, SourceContext context) : this(null, null, body, context) { }

        public CreatorContext(TypenameExpression typecast, SourceContext context) : this(null, typecast, new BaseStatement[0], context) { }

        public CreatorContext(NameExpression name, TypenameExpression typecast, BaseStatement[] body, SourceContext context) : base(context)
        {
            Name = name;
            Typecast = typecast;
            Body = body;
        }

        public override string ExpressionToString() => $"new context {(HasName ? Name.Name : "")} {(HasCast ? $": {Typecast}" : "")} {{\n{Body.JoinIntoString("\n")}\n}}";

        private string last_context_name;
        public string LastContextName => last_context_name;

        public override object GetValue(ExecutionContext context)
        {
            var ret = new ExecutionContext(context);
            if (HasName) ret.Name = Name.Name;
            last_context_name = ret.Name;
            if (HasBody)
            {
                for (var i = 0; i < Body.Length; i++)
                {
                    if (Body[i] is ExpressionStatement es && es.Expression is BinaryAssign assign)
                        assign.AssignValue(ret, assign.Left, assign.Right.GetValue(context));
                    else if (Body[i] is ContextStatement cs)
                        cs.GetValue(ret);
                }
            }
            if (HasCast) return new ContextWrap(ret).CastToType(Typecast.GetValue(context).Cast<Type>());
            return new ContextWrap(ret);
        }

        public override object Clone() => new CreatorContext(Name.CloneCast(), Typecast.CloneCast(), Body?.CloneArray(), SourceContext.CloneCast());
    }
}
