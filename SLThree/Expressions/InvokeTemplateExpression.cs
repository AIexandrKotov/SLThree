using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class InvokeTemplateExpression : BaseExpression
    {
        public class GenericMakingDefinition : BaseExpression
        {
            public BaseExpression Expression;

            public GenericMakingDefinition(BaseExpression expression, SourceContext context) : base(context)
            {
                Expression = expression;
            }

            public override string ExpressionToString() => Expression.ToString();

            public override object GetValue(ExecutionContext context)
            {
                if (Expression is NameExpression name) 
                {
                    switch (name.Name) 
                    {
                        case "value":
                            return TemplateMethod.GenericMaking.AsValue;
                        case "name":
                            return TemplateMethod.GenericMaking.AsName;
                        case "type":
                            return TemplateMethod.GenericMaking.AsType;
                        case "expr":
                            return TemplateMethod.GenericMaking.AsExpression;
                        case "code":
                            return TemplateMethod.GenericMaking.AsCode;
                        case "constraint":
                            return TemplateMethod.GenericMaking.AsConstraint;
                        case "runtime":
                            return TemplateMethod.GenericMaking.Constraint;
                        case "runtime2":
                            return TemplateMethod.GenericMaking.Runtime;
                    }
                }
                return Expression.GetValue(context);
            }

            public override object Clone()
            {
                return new GenericMakingDefinition(Expression.CloneCast(), SourceContext.CloneCast());
            }
        }

        public BaseExpression Left;
        public (BaseExpression, BaseExpression)[] GenericArguments;
        public BaseExpression[] Arguments;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        public InvokeTemplateExpression(BaseExpression left, (BaseExpression, BaseExpression)[] genericArguments, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
        }
        public InvokeTemplateExpression(BaseExpression left, (BaseExpression, BaseExpression)[] genericArguments, BaseExpression[] arguments, bool null_conditional, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? ".?" : "")}<{GenericArguments.JoinIntoString(", ")}>({Arguments.JoinIntoString(", ")})";

        public object InvokeForObj(ExecutionContext context, (TemplateMethod.GenericMaking, object)[] generic_args, object[] args, object o)
        {
            if (o == null)
            {
                if (null_conditional) return null;
                throw new RuntimeError($"Method `{Left}` not found", SourceContext);
            }

            if (o is Method method)
            {
                if (args.Length < method.RequiredArguments || args.Length > method.MaximumArguments) throw new RuntimeError("Call with wrong arguments count", SourceContext);
                if (o is TemplateMethod template_method)
                    return template_method.MakeGenericMethod(generic_args).GetValue(context, args);

                throw new NotSupportedException("Generic invokation for SLThree methods is not supported");
            }

            throw new RuntimeError($"{o.GetType().GetTypeString()} is not allow to making generic", SourceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(ExecutionContext context, (TemplateMethod.GenericMaking, object)[] generic_args, object[] args)
        {
            return InvokeForObj(context, generic_args, args, Left.GetValue(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, GenericArguments.ConvertAll(x => ((TemplateMethod.GenericMaking)x.Item1.GetValue(context), x.Item2.GetValue(context))), Arguments.ConvertAll(x => x.GetValue(context)));
        }

        public override object Clone()
        {
            return new InvokeTemplateExpression(Left.CloneCast(), GenericArguments.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())), Arguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}
