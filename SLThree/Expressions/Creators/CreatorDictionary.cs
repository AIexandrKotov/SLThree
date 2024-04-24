using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class CreatorDictionary : BaseExpression
    {
        public TypenameExpression[] DictionaryType;
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

        public Entry[] Entries;

        public CreatorDictionary(Entry[] entries, SourceContext context) : this(entries, null, context) { }
        public CreatorDictionary(Entry[] entries, TypenameExpression[] type, SourceContext context) : base(context)
        {
            Entries = entries;
            DictionaryType = type;
        }

        public override object GetValue(ExecutionContext context)
        {
            if (DictionaryType == null)
            {
                return Entries.Select(x => ((object, object))x.GetValue(context)).ToDictionary(x => x.Item1, x => x.Item2);
            }
            else
            {
                return GetTypedDictionaryMethod.MakeGenericMethod(DictionaryType.ConvertAll(x => (Type)x.GetValue(context))).Invoke(null, new object[2] { Entries, context });
            }
        }

        private static MethodInfo GetTypedDictionaryMethod = typeof(CreatorDictionary).GetMethod("GetTypedDictionary", BindingFlags.Static | BindingFlags.NonPublic);
#pragma warning disable IDE0051 // Удалите неиспользуемые закрытые члены
        private static Dictionary<TKey, TValue> GetTypedDictionary<TKey, TValue>(IEnumerable<BaseExpression> expressions, ExecutionContext context)
            =>
            context.ForbidImplicit
            ? expressions.Select(x => ((object, object))x.GetValue(context)).ToDictionary(x => x.Item1.Cast<TKey>(), x => x.Item2.Cast<TValue>())
            : expressions.Select(x => ((object, object))x.GetValue(context)).ToDictionary(x => x.Item1.CastToType<TKey>(), x => x.Item2.CastToType<TValue>());
#pragma warning restore IDE0051 // Удалите неиспользуемые закрытые члены

        public override string ExpressionToString() => $"{{{Entries.JoinIntoString(", ")}}}";

        public override object Clone()
        {
            return new CreatorDictionary(Entries.CloneArray(), DictionaryType?.CloneArray(), SourceContext.CloneCast());
        }
    }
}
