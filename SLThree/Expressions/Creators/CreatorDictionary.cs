using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

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
                return new KeyValuePair<object, object>(Key.GetValue(context), Value.GetValue(context));
            }
            public override string ExpressionToString() => $"{Key}: {Value}";
            public override object Clone()
            {
                return new Entry(Key.CloneCast(), Value.CloneCast(), SourceContext.CloneCast());
            }
        }

        public Entry[] Entries;

        public CreatorDictionary(Entry[] entries, SourceContext context) : base(context)
        {
            Entries = entries;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Entries.Select(x => (KeyValuePair<object, object>)x.GetValue(context)).ToDictionary(x => x.Key, x => x.Value);
        }

        public override string ExpressionToString() => $"{{{Entries.JoinIntoString(", ")}}}";

        public override object Clone()
        {
            return new CreatorDictionary(Entries.CloneArray(), SourceContext.CloneCast());
        }
    }
}
