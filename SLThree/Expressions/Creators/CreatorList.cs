using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree
{

    public class CreatorList : BaseExpression
    {
        public TypenameExpression ListType;
        public BaseExpression[] Expressions;

        public CreatorList(BaseExpression[] expressions, TypenameExpression type, SourceContext context) : base(context)
        {
            ListType = type;
            Expressions = expressions;
        }
        public CreatorList(BaseExpression[] expressions, SourceContext context) : this(expressions, null, context) { }

        public override object GetValue(ExecutionContext context)
        {
            if (ListType == null)
                return Expressions.Select(x => x.GetValue(context)).ToList();
            else
            {
                return GetTypedListMethod.MakeGenericMethod(new Type[1] { (Type)ListType.GetValue(context) })
                    .Invoke(null, new object[2] { Expressions, context });
            }
        }

        private static MethodInfo GetTypedListMethod = typeof(CreatorList).GetMethod("GetTypedList", BindingFlags.Static | BindingFlags.NonPublic);
#pragma warning disable IDE0051 // Удалите неиспользуемые закрытые члены
        private static List<T> GetTypedList<T>(IEnumerable<BaseExpression> expressions, ExecutionContext context)
            => context.ForbidImplicit 
            ? expressions.Select(x => x.GetValue(context).Cast<T>()).ToList()
            : expressions.Select(x => x.GetValue(context).CastToType<T>()).ToList();
#pragma warning restore IDE0051 // Удалите неиспользуемые закрытые члены

        public override string ExpressionToString() => $"{(ListType == null ? "" : $"<{ListType}>")}[{Expressions.JoinIntoString(", ")}]";

        public override object Clone()
        {
            return new CreatorList(Expressions.CloneArray(), ListType.CloneCast(), SourceContext);
        }
    }
}
