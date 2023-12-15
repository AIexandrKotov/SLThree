using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static SLThree.SwitchStatement;

namespace SLThree
{

    public class ReflectionExpression : BaseExpression
    {
        public BaseExpression Left;
        public NameExpression Right;
        public TypenameExpression[] MethodGenericArguments;
        public TypenameExpression[] MethodArguments;


        private string name;

        //Left::Right<MethodGenericArguments>(MethodArguments)
        //Left::Right(MethodArguments)
        //Left::Right

        public ReflectionExpression(BaseExpression left, NameExpression right, TypenameExpression[] methodGenericArguments, TypenameExpression[] methodArguments, SourceContext context) : base(context)
        {
            Left = left;
            Right = right;
            MethodGenericArguments = methodGenericArguments;
            MethodArguments = methodArguments;
            name = Right.ToString();
        }

        public ReflectionExpression(BaseExpression left, NameExpression right, TypenameExpression[] methodArguments, SourceContext context) : this(left, right, null, methodArguments, context) { }

        public ReflectionExpression(BaseExpression left, NameExpression right, SourceContext context) : this(left, right, null, null, context) { }

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
                var method = left.GetMethod(name, MethodArguments.ConvertAll(x => x.GetValue(context).Cast<Type>()));
                if (MethodGenericArguments != null) method.MakeGenericMethod(MethodGenericArguments.ConvertAll(x => x.GetValue(context).Cast<Type>()));
                return method;
            }
            throw new RuntimeError($"{this} not found", SourceContext);
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
