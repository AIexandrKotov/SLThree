using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SLThree
{
    public class CreatorDictionary : BaseExpression
    {
        public class Entry : BaseExpression
        {
            public BaseExpression Key;
            public BaseExpression Value;
            public Entry(BaseExpression key, BaseExpression value, SourceContext context) : base(context)
            {
                Key = key;
                Value = value;
            }
            public override object GetValue(ExecutionContext context)
            {
                return (Key.GetValue(context), Value.GetValue(context));
            }
            public override string ExpressionToString() => $"{Key}: {Value}";
            public override object Clone()
            {
                return new Entry(Key.CloneCast(), Value.CloneCast(), SourceContext.CloneCast());
            }
        }

        /*
        ---Creating dictionaries---
        new T {:};
        new T(args) {:};
        ---With name assignation
        new T Name {:};
        new T(args) Name {:};
        */
        public static CreatorDictionary CaseTypeBody(TypenameExpression type, Entry[] body, SourceContext context)
            => new CreatorDictionary(
                type,
                null,
                new BaseExpression[0],
                body,
                context);
        public static CreatorDictionary CaseTypeArgsBody(TypenameExpression type, BaseExpression[] args, Entry[] body, SourceContext context)
            => new CreatorDictionary(
                type,
                null,
                args,
                body,
                context);
        public static CreatorDictionary NamedCaseTypeBody(TypenameExpression type, BaseExpression name, Entry[] body, SourceContext context)
            => new CreatorDictionary(
                type,
                name,
                new BaseExpression[0],
                body,
                context);
        public static CreatorDictionary NamedCaseTypeArgsBody(TypenameExpression type, BaseExpression name, BaseExpression[] args, Entry[] body, SourceContext context)
            => new CreatorDictionary(
                type,
                name,
                args,
                new Entry[0],
                context);


        public CreatorDictionary(TypenameExpression type, BaseExpression name, BaseExpression[] args, Entry[] body, SourceContext context) : base(context)
        {
            Type = type;
            Name = name;
            Arguments = args;
            Body = body;
        }

        public TypenameExpression Type;
        public BaseExpression Name { get; set; }
        public BaseExpression[] Arguments { get; set; }
        public Entry[] Body { get; set; }

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

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            object instance;
            var type = Type.GetValue(context).Cast<Type>();
            if (!type.IsDictionary()) throw new RuntimeError($"The dictionary initializer cannot be used for the type {type.GetTypeString()}", SourceContext);
            instance = Activator.CreateInstance(type, Arguments.ConvertAll(x => x.GetValue(context)));
            if (Name != null)
                BinaryAssign.AssignToValue(context, Name, instance, ref counted_invoked, ref is_name_expr, ref variable_index);
            return GetTypedDictionaryMethod.MakeGenericMethod(type.GetGenericArguments()).Invoke(null, new object[] { Body, instance, context }); ;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(IEnumerable<(object, object)> source, Dictionary<TKey, TElement> target, bool forbid_implicit)
        {
            var dictionary = target ?? new Dictionary<TKey, TElement>();
            if (forbid_implicit)
            {
                foreach (var item in source)
                    dictionary.Add(item.Item1.Cast<TKey>(), item.Item2.Cast<TElement>());
            }
            else
            {
                foreach (var item in source)
                    dictionary.Add(item.Item1.CastToType<TKey>(), item.Item2.CastToType<TElement>());
            }
            return dictionary;
        }
        private static MethodInfo GetTypedDictionaryMethod = typeof(CreatorDictionary).GetMethod("GetTypedDictionary", BindingFlags.Static | BindingFlags.NonPublic);
#pragma warning disable IDE0051 // Удалите неиспользуемые закрытые члены
        private static Dictionary<TKey, TValue> GetTypedDictionary<TKey, TValue>(IEnumerable<BaseExpression> expressions, Dictionary<TKey, TValue> target, ExecutionContext context)
            => ToDictionary(expressions.Select(x => ((object, object))x.GetValue(context)), target, context.ForbidImplicit);
#pragma warning restore IDE0051 // Удалите неиспользуемые закрытые члены

        public override object Clone()
        {
            return new CreatorDictionary(Type.CloneCast(), Name.CloneCast(), Arguments.CloneArray(), Body.CloneArray(), SourceContext.CloneCast());
        }
    }
}
