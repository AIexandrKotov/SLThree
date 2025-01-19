using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class ReflectionExpression : BaseExpression
    {
        public override int Priority => 0;
        public BaseExpression Left;
        public NameExpression Right;
        public TypenameExpression[] MethodGenericArguments;
        public TypenameExpression[] MethodArguments;


        private string name;
        private bool is_constructor;

        //Left::Right<MethodGenericArguments>(MethodArguments)
        //Left::Right(MethodArguments)
        //Left::Right

        public ReflectionExpression(BaseExpression left, NameExpression right, TypenameExpression[] methodGenericArguments, TypenameExpression[] methodArguments, ISourceContext context) : base(context)
        {
            Left = left;
            Right = right;
            MethodGenericArguments = methodGenericArguments;
            MethodArguments = methodArguments;
            name = Right.ToString();
            is_constructor = name == "new";
        }

        public ReflectionExpression(BaseExpression left, NameExpression right, TypenameExpression[] methodArguments, ISourceContext context) : this(left, right, null, methodArguments, context) { }

        public ReflectionExpression(BaseExpression left, NameExpression right, ISourceContext context) : this(left, right, null, null, context) { }

        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).Cast<Type>();
            if (MethodArguments == null)
            {
                var field = left.GetField(name);
                if (field != null) return field;
                var property = left.GetProperty(name);
                if (property != null) return property;
            }
            else
            {
                if (is_constructor)
                {
                    var method = left.GetConstructor(MethodArguments.ConvertAll(x => x.GetValue(context).Cast<Type>()));
                    return method;
                }
                else
                {
                    var method = left.GetMethod(name, MethodArguments.ConvertAll(x => x.GetValue(context).Cast<Type>()));
                    if (MethodGenericArguments != null) method = method.MakeGenericMethod(MethodGenericArguments.ConvertAll(x => x.GetValue(context).Cast<Type>()));
                    return method;
                }
            }
            throw new ReflectionNotFound(this, SourceContext);
        }

        public override object Clone()
        {
            return new ReflectionExpression(Left.CloneCast(), Right.CloneCast(), MethodGenericArguments?.CloneArray(), MethodArguments?.CloneArray(), SourceContext.CloneCast());
        }

        public override string ExpressionToString()
        {
            return $"{Left}::{Right}{(MethodGenericArguments == null ? "" : $"<{MethodGenericArguments.JoinIntoString(", ")}>")}{(MethodArguments == null ? "" : $"({MethodArguments.JoinIntoString(", ")})")}";
        }
    }
}
