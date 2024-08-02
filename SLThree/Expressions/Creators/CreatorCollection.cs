using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SLThree
{
    public class CreatorCollection : BaseExpression
    {
        /*
        ---Creating collections ---
        new {,,};
        new T {,,};
        new T(args) {,,};
        ---With name assignation
        new T Name {,,};
        new T(args) Name {,,};
        */
        public TypenameExpression Type;
        public BaseExpression Name;
        public BaseExpression[] Arguments;
        public BaseExpression[] Body;
        public static CreatorCollection CaseShort(BaseExpression[] body, SourceContext context)
            => new CreatorCollection(
                null,
                null,
                new BaseExpression[0],
                body,
                context);
        public static CreatorCollection CaseTypeBody(TypenameExpression type, BaseExpression[] body, SourceContext context)
            => new CreatorCollection(
                type,
                null,
                new BaseExpression[0],
                body,
                context);
        public static CreatorCollection CaseTypeArgsBody(TypenameExpression type, BaseExpression[] args, BaseExpression[] body, SourceContext context)
            => new CreatorCollection(
                type,
                null,
                args,
                body,
                context);
        public static CreatorCollection NamedCaseTypeBody(TypenameExpression type, BaseExpression name, BaseExpression[] body, SourceContext context)
            => new CreatorCollection(
                type,
                name,
                new BaseExpression[0],
                body,
                context);
        public static CreatorCollection NamedCaseTypeArgsBody(TypenameExpression type, BaseExpression name, BaseExpression[] args, BaseExpression[] body, SourceContext context)
            => new CreatorCollection(
                type,
                name,
                args,
                body,
                context);

        public CreatorCollection(TypenameExpression type, BaseExpression name, BaseExpression[] arguments, BaseExpression[] body, SourceContext context) : base(context)
        {
            Type = type ?? new TypenameExpression(new NameExpression("array", context), context);
            Name = name;
            Arguments = arguments;
            Body = body;
        }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();
            sb.Append("new ");
            sb.Append(Type.ToString());
            if (Arguments.Length > 0)
                sb.Append($"({Arguments.JoinIntoString(", ")})");
            if (Name != null)
                sb.Append($" {Name}");
            if (Body.Length > 0)
            {
                sb.Append($"{{\n{Body.JoinIntoString(", ")}\n}}");
            }
            return sb.ToString();
        }

        public override object GetValue(ExecutionContext context)
        {
            object instance;
            var type = Type.GetValue(context).Cast<Type>();
            Type element_type;
            if (type.IsArray)
            {
                element_type = type.GetElementType();
                if (Arguments.Length != 0) instance = Array.CreateInstance(type.GetElementType(), Arguments.ConvertAll(x => x.GetValue(context).CastToType<int>()));
                else instance = Array.CreateInstance(element_type, Body.Length);
            }
            else
            {
                instance = Activator.CreateInstance(type, Arguments.ConvertAll(x => x.GetValue(context)));
                element_type = type.GetGenericArguments()[0];
            }

            if (Name != null)
                BinaryAssign.AssignToValue(context, Name, instance, ref counted_invoked, ref is_name_expr, ref variable_index);

            if (instance is Array)
                return GetTypedArrayMethod.MakeGenericMethod(element_type).Invoke(null, new object[] { Body, instance, context });
            if (type.IsList())
                return GetTypedListMethod.MakeGenericMethod(element_type).Invoke(null, new object[] { Body, instance, context });
            if (type.IsStack())
                return GetTypedStackMethod.MakeGenericMethod(element_type).Invoke(null, new object[] { Body, instance, context });
            if (type.IsQueue())
                return GetTypedQueueMethod.MakeGenericMethod(element_type).Invoke(null, new object[] { Body, instance, context });

            throw new RuntimeError($"The collection initializer cannot be used for the type {type.GetTypeString()}", SourceContext);
        }

        private static MethodInfo
            GetTypedArrayMethod = typeof(CreatorCollection).GetMethod("GetTypedArray", BindingFlags.Static | BindingFlags.NonPublic),
            GetTypedListMethod = typeof(CreatorCollection).GetMethod("GetTypedList", BindingFlags.Static | BindingFlags.NonPublic),
            GetTypedStackMethod = typeof(CreatorCollection).GetMethod("GetTypedStack", BindingFlags.Static | BindingFlags.NonPublic),
            GetTypedQueueMethod = typeof(CreatorCollection).GetMethod("GetTypedQueue", BindingFlags.Static | BindingFlags.NonPublic);

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;
#pragma warning disable IDE0051 // Удалите неиспользуемые закрытые члены
        private static T[] GetTypedArray<T>(IEnumerable<BaseExpression> expressions, T[] target, ExecutionContext context)
            => context.ForbidImplicit
            ? expressions.Select(x => x.GetValue(context).Cast<T>()).ToArray()
            : expressions.Select(x => x.GetValue(context).CastToType<T>()).ToArray();
        private static List<T> GetTypedList<T>(IEnumerable<BaseExpression> expressions, ExecutionContext context)
            => context.ForbidImplicit
            ? expressions.Select(x => x.GetValue(context).Cast<T>()).ToList()
            : expressions.Select(x => x.GetValue(context).CastToType<T>()).ToList();
        private static Stack<T> GetTypedStack<T>(IEnumerable<BaseExpression> expressions, Stack<T> target, ExecutionContext context)
            => context.ForbidImplicit
            ? new Stack<T>(expressions.Select(x => x.GetValue(context).Cast<T>()))
            : new Stack<T>(expressions.Select(x => x.GetValue(context).CastToType<T>()).ToList());
        private static Queue<T> GetTypedQueue<T>(IEnumerable<BaseExpression> expressions, Queue<T> target, ExecutionContext context)
            => context.ForbidImplicit
            ? new Queue<T>(expressions.Select(x => x.GetValue(context).Cast<T>()))
            : new Queue<T>(expressions.Select(x => x.GetValue(context).CastToType<T>()).ToList());
#pragma warning restore IDE0051 // Удалите неиспользуемые закрытые члены

        public override object Clone()
        {
            return new CreatorCollection(Type.CloneCast(), Name.CloneCast(), Arguments.CloneArray(), Body.CloneArray(), SourceContext.CloneCast());
        }
    }
}
