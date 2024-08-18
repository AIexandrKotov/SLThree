using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class IndexExpression : BaseExpression
    {
        public BaseExpression Expression;
        public BaseExpression[] Arguments;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        public IndexExpression(BaseExpression expression, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Expression = expression;
            Arguments = arguments;
        }
        public IndexExpression(BaseExpression expression, BaseExpression[] arguments, bool null_conditional, SourceContext context) : base(context)
        {
            Expression = expression;
            Arguments = arguments;
            this.null_conditional = null_conditional;
        }

        private int Mode = 0; // 1 - array, 2 - list, 3 - tuple, 4 - any
        private PropertyInfo PropertyInfo;

        private void CalcMode(object o)
        {
            var type = o.GetType();
            Mode = type.IsArray ? 1 : 0;
            if (Mode == 0)
            {
                Mode = type.IsList() ? 1 : 0;
                if (Mode == 0)
                {
                    Mode = type.GetInterfaces().Any(x => x == typeof(ITuple)) ? 3 : 0;
                    if (Mode == 0)
                    {
                        while (PropertyInfo == null && type != null)
                        {
                            PropertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.GetIndexParameters().Length == Arguments.Length);
                            if (PropertyInfo == null) type = type.BaseType;
                        }
                        if (PropertyInfo == null)
                        {
                            Mode = int.MaxValue;
                        }
                        else Mode = 4;
                    }
                }
            }
        }

        public override object GetValue(ExecutionContext context)
        {
            var o = Expression.GetValue(context);

            if (o == null)
            {
                if (null_conditional) return null;
                throw new IndexTargetWasNull(Expression, SourceContext);
            }

            if (Mode == 0)
            {
                CalcMode(o);
            }

            switch (Mode)
            {
                case 1:
                    {
                        return o.Cast<IList>()[context.ForbidImplicit ? Arguments[0].GetValue(context).Cast<int>() : Arguments[0].GetValue(context).CastToType(typeof(int)).Cast<int>()];
                    }
                case 3:
                    {
                        return o.Cast<ITuple>()[context.ForbidImplicit ? Arguments[0].GetValue(context).Cast<int>() : Arguments[0].GetValue(context).CastToType(typeof(int)).Cast<int>()];
                    }
                case 4: return PropertyInfo.GetValue(o, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            return null;
        }

        public object SetValue(ExecutionContext context, object value)
        {
            var o = Expression.GetValue(context);

            if (o == null) return null;

            if (Mode == 0)
            {
                CalcMode(o);
            }

            switch (Mode)
            {
                case 1:
                    {
                        return o.Cast<IList>()[context.ForbidImplicit ? Arguments[0].GetValue(context).Cast<int>() : Arguments[0].GetValue(context).CastToType(typeof(int)).Cast<int>()] = value;
                    }
                case 4:
                    PropertyInfo.SetValue(o, value, Arguments.ConvertAll(x => x.GetValue(context)));
                    break;
            }
            return value;
        }

        public override string ExpressionToString() => $"{Expression}{(null_conditional ? ".?" : "")}[{Arguments.JoinIntoString(", ")}]";

        public override object Clone()
        {
            return new IndexExpression(Expression.CloneCast(), Arguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}
