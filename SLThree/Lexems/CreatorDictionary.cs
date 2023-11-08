using Pegasus.Common;
using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class CreatorDictionary : BaseLexem
    {
        public class Entry : BaseLexem
        {
            public BaseLexem Key;
            public BaseLexem Value;
            public Entry(BaseLexem key, BaseLexem value, Cursor cursor) : base(cursor)
            {
                Key = key;
                Value = value;
            }
            public override object GetValue(ExecutionContext context)
            {
                return new KeyValuePair<object, object>(Key.GetValue(context), Value.GetValue(context));
            }
            public override string ToString() => $"{Key}: {Value}";
        }

        public Entry[] Entries;
        
        public CreatorDictionary(Entry[] entries, Cursor cursor) : base(cursor)
        {
            Entries = entries;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Entries.Select(x => (KeyValuePair<object, object>)x.GetValue(context)).ToDictionary(x => x.Key, x => x.Value);
        }

        public override string ToString() => $"{{{Entries.JoinIntoString(", ")}}}";
    }
}
