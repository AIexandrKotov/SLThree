﻿using Pegasus.Common;
using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLThree
{
    public class InterpolatedString : BaseLexem
    {
        public string Value { get; set; }
        public BaseLexem[] Lexems { get; set; }

        public InterpolatedString(string value, IList<(BaseLexem, string)> other, Cursor cursor) : base(cursor)
        {
            var sb = new StringBuilder();
            Lexems = new BaseLexem[other.Count];
            sb.Append(value);
            for (var i = 0; i < other.Count; i++)
            {
                Lexems[i] = other[i].Item1;
                sb.Append($"{{{i}}}");
                sb.Append(other[i].Item2);
            }
            Value = sb.ToString();
        }

        public override string ToString() => string.Format(Value, Lexems.ConvertAll(x => x.ToString()));

        public override object GetValue(ExecutionContext context)
        {
            return string.Format(Value, Lexems.ConvertAll(x => x.GetValue(context)));
        }
    }
}
