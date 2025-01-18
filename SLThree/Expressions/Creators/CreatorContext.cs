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
        /// <summary>
        /// Не является частью CreateInstance
        /// </summary>
        public bool IsFreeCreator { get; set; }
        public bool GeneratePrivate { get; set; }

        public bool HasName => Name != null;

        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, CreatorContextBody body, bool is_free_creator, ISourceContext context, bool generatePrivate = true) : base(context)
        {
            Name = name;
            Ancestors = ancestors;
            CreatorBody = body;
            IsFreeCreator = is_free_creator;
            GeneratePrivate = generatePrivate;
        }
        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, CreatorContextBody body, ISourceContext context)
            : this(name, ancestors, body, true, context) { }
        public CreatorContext(BaseExpression name, CreatorContextBody body, ISourceContext context)
            : this(name, new BaseExpression[0], body, true, context) { }
        public CreatorContext(BaseExpression[] ancestors, CreatorContextBody body, ISourceContext context)
            : this(null, ancestors, body, true, context) { }
        public CreatorContext(CreatorContextBody body, ISourceContext context, bool generatePrivate = true)
            : this(null, new BaseExpression[0], body, true, context, generatePrivate) { }
        public CreatorContext(BaseExpression name, BaseExpression[] ancestors, ISourceContext context)
            : this(name, ancestors, null, true, context) { }
        public CreatorContext(BaseExpression name, ISourceContext context)
            : this(name, new BaseExpression[0], null, true, context) { }
        public CreatorContext(BaseExpression[] ancestors, ISourceContext context)
            : this(null, ancestors, null, true, context) { }
        public CreatorContext(ISourceContext context)
            : this(null, new BaseExpression[0], null, true, context) { }

        public override string ExpressionToString() => $"context {(HasName ? Name.ToString() : "")} {{\n{CreatorBody}\n}}";

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        private string GetName() => GetLastName(Name);
        public static string GetLastName(BaseExpression name)
        {
            name.PrioriryRaised = false;
            var n = name.ToString();
            var index = n.LastIndexOf('.');
            if (index == -1) return n;
            else return n.Substring(index + 1);
        }

        public object GetValue(ExecutionContext target, ExecutionContext context)
        {
            var ret = new ExecutionContext(target, GeneratePrivate);
            for (var i = 0; i < Ancestors.Length; i++)
            {
                var ancestor = Ancestors[i].GetValue(target).Cast<ContextWrap>() ?? throw new System.Exception("Null inheritance");
                ret.implement(ancestor.Context);
            }
            CreatorBody?.GetValue(ret, context);
            var wrap = ret.wrap;
            if (HasName)
            {
                ret.Name = GetName();
                if (IsFreeCreator)
                    BinaryAssign.AssignToValue(target, Name, wrap, ref counted_invoked, ref is_name_expr, ref variable_index);
            }
            return wrap;
        }

        public override object GetValue(ExecutionContext context) => GetValue(context, context);

        public override object Clone() => new CreatorContext(Name.CloneCast(), Ancestors.CloneArray(), CreatorBody.CloneCast(), IsFreeCreator, SourceContext.CloneCast(), GeneratePrivate);
    }
}
