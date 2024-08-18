using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class MakeTemplateExpression : BaseExpression
    {
        public BaseExpression Left;
        public (BaseExpression, BaseExpression)[] GenericArguments;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        public MakeTemplateExpression(BaseExpression left, (BaseExpression, BaseExpression)[] genericArguments, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
        }
        public MakeTemplateExpression(BaseExpression left, (BaseExpression, BaseExpression)[] genericArguments, bool null_conditional, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? ".?" : "")}<{GenericArguments.JoinIntoString(", ")}>";

        public object InvokeForObj(ExecutionContext context, (TemplateMethod.GenericMaking, object)[] generic_args, object o)
        {
            if (o == null)
            {
                if (null_conditional) return null;
                throw new MethodNotFound(Left, SourceContext);
            }

            if (o is Method method)
            {
                if (o is TemplateMethod generic_method)
                    return generic_method.MakeGenericMethod(generic_args);

                throw new RuntimeError("Generic invokation for SLThree methods is not supported", SourceContext);
            }

            throw new MakeNotAllow(o.GetType(), SourceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(ExecutionContext context, (TemplateMethod.GenericMaking, object)[] generic_args)
        {
            return InvokeForObj(context, generic_args, Left.GetValue(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, GenericArguments.ConvertAll(x => ((TemplateMethod.GenericMaking)x.Item1.GetValue(context), x.Item2.GetValue(context))));
        }

        public override object Clone()
        {
            return new MakeTemplateExpression(Left.CloneCast(), GenericArguments.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())), null_conditional, SourceContext.CloneCast());
        }
    }
}
