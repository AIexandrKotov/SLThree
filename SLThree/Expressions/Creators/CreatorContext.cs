using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class CreatorContext : BaseExpression
    {
        /*
        ---Creating instances---
        new context Name: TBase
        new context Name
        new context: TBase
        new context

        context Name: TBase {}
        context Name {}
        context: TBase {}
        context {}
        */
        public BaseExpression Name { get; set; }
        public BaseExpression[] Ancestors { get; set; }
        public CreatorContextBody CreatorBody { get; set; }
        public bool IsFreeCreator { get; set; }

        public bool HasName => Name != null;

        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, CreatorContextBody body, bool is_free_creator, SourceContext context) : base(context)
        {
            if (!is_free_creator)
            {
                if (!is_free_creator && name is MemberAccess access)
                    Name = access.Right as NameExpression;
                else Name = name as NameExpression;
            }
            else Name = name;
            Ancestors = ancestors;
            CreatorBody = body;
            IsFreeCreator = is_free_creator;
        }
        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            : this(name, ancestors, body, true, context) { }
        public CreatorContext(BaseExpression name, CreatorContextBody body, SourceContext context)
            : this(name, new BaseExpression[0], body, true, context) { }
        public CreatorContext(BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            : this(null, ancestors, body, true, context) { }
        public CreatorContext(CreatorContextBody body, SourceContext context)
            : this(null, new BaseExpression[0], body, true, context) { }
        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, SourceContext context)
            : this(name, ancestors, null, true, context) { }
        public CreatorContext(BaseExpression name, SourceContext context)
            : this(name, new BaseExpression[0], null, true, context) { }
        public CreatorContext(BaseExpression[] ancestors, SourceContext context)
            : this(null, ancestors, null, true, context) { }
        public CreatorContext(SourceContext context)
            : this(null, new BaseExpression[0], null, true, context) { }

        public override string ExpressionToString() => $"context {(HasName ? Name.ToString() : "")} {{\n{CreatorBody}\n}}";

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        private string GetName()
        {
            var n = Name.ToString();
            var index = n.LastIndexOf('.');
            if (index == -1) return n;
            else return n.Substring(index + 1);
        }
        public override object GetValue(ExecutionContext context)
        {
            ExecutionContext ret;
            if (Ancestors.Length > 0) ret = Ancestors[0].GetValue(context).Cast<ContextWrap>().Context;
            else ret = new ExecutionContext(context);
            for (var i = 1; i < Ancestors.Length; i++)
                ret.copy(Ancestors[i].GetValue(context).Cast<ContextWrap>().Context);
            CreatorBody?.GetValue(ret, context);
            var wrap = ret.wrap;
            if (HasName)
            {
                ret.Name = IsFreeCreator ? GetName() : Name.ToString();
                if (IsFreeCreator)
                    BinaryAssign.AssignToValue(context, Name, wrap, ref counted_invoked, ref is_name_expr, ref variable_index);
            }
            return wrap;
        }

        public override object Clone() => new CreatorContext(Name.CloneCast(), Ancestors.CloneArray(), CreatorBody.CloneCast(), IsFreeCreator, SourceContext.CloneCast());
    }
}
