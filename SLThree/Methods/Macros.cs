using SLThree.Extensions.Cloning;
using SLThree.Visitors;
using System;
using System.Collections.Generic;

namespace SLThree
{
    public class Macros : ExecutionContext.IExecutable
    {
        public class MacrosElement : BaseExpression
        {
            public Macros Macros;
            public int Id;

            public MacrosElement(int id, SourceContext context) : base(context)
            {
                if (id > 255) throw new ArgumentException(nameof(id), "The number of macro arguments is limited to 256");
                Id = id;
            }

            public MacrosElement Bind(Macros macros)
            {
                Macros = macros;
                return this;
            }

            public override string ExpressionToString() => $"${Id}";

            public override object GetValue(ExecutionContext context)
            {
                if (Macros != null)
                    return Macros.elements[Id];
                throw new RuntimeError("The macros argument was outside the macros", SourceContext);
            }

            public override object Clone()
            {
                return new MacrosElement(Id, SourceContext.CloneCast()) { Macros = Macros };
            }
        }

        public readonly ExecutionContext.IExecutable content;
        public readonly ExecutionContext Context;
        public object[] elements;

        public class Macrositor : AbstractVisitor
        {
            public readonly Macros macros;
            internal int max = -1;

            public Macrositor(Macros macros)
            {
                this.macros = macros;
            }

            public override void VisitExpression(BaseExpression expression)
            {
                if (expression is MacrosElement element)
                {
                    element.Bind(macros);
                    if (element.Id > max) max = element.Id;
                }
                base.VisitExpression(expression);
            }

            public static int AssignAllArgs(ExecutionContext.IExecutable executable, Macros macros)
            {
                var mv = new Macrositor(macros);
                mv.VisitAny(executable);
                return mv.max;
            }
        }

        public Macros(ExecutionContext.IExecutable executable, ExecutionContext context)
        {
            content = executable;
            Context = context;
            elements = new object[Macrositor.AssignAllArgs(content, this) + 1];
            invoke = new Method("invoke", new string[1] { "args" }, new StatementList(new BaseStatement[]
            {
                new ReturnStatement(new MemberAccess(new StaticExpression(this), new InvokeExpression(new NameExpression("Invoke", null), new BaseExpression[]{new NameExpression("args", null) }, null), null), null)
            }, null), new TypenameExpression[] { null }, null, context.wrap, true, false, false, new BaseExpression[0]); ;
        }

        public object GetValue(ExecutionContext _)
        {
            return content.GetValue(Context);
        }

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;
        public object set(object o)
        {
            if (content is NameExpression nameExpression)
                return BinaryAssign.AssignToValue(Context, nameExpression, o, ref counted_invoked, ref is_name_expr, ref variable_index);
            throw new NotSupportedException("Macros.set support only names");
        }
        public readonly Method invoke;
        public object Invoke(object[] args)
        {
            args.CopyTo(elements, 0);
            return content.GetValue(Context);
        }

        public override string ToString() => "$" + (content is BaseStatement ? $"{{{content}}}" : $"({content})");
    }
}
