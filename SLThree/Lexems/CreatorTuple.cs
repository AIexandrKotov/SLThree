using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class CreatorTuple : BaseLexem
    {
        public BaseLexem[] Lexems;
        
        public CreatorTuple(BaseLexem[] lexems, SourceContext context) : base(context)
        {
            Lexems = lexems;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Create(Lexems.ConvertAll(x => x.GetValue(context)));
        }

        public static ITuple Create(object[] objs, int index = 0)
        {
            switch (objs.Length - index)
            {
                case 0: throw new ArgumentException("Zero length array in objs");
                case 1: return new Tuple<object>(objs[index]);
                case 2: return new Tuple<object, object>(objs[index], objs[index + 1]);
                case 3: return new Tuple<object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2]);
                case 4: return new Tuple<object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3]);
                case 5: return new Tuple<object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4]);
                case 6: return new Tuple<object, object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5]);
                case 7: return new Tuple<object, object, object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5], objs[index + 6]);
                default: return new Tuple<object, object, object, object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5], objs[index + 6], Create(objs, index + 7));
            }
        }
        public static object[] ToArray(ITuple tuple)
        {
            var ret = new object[tuple.Length];
            for (var i = 0; i < tuple.Length; i++)
                ret[i] = tuple[i];
            return ret;
        }
        public override string LexemToString() => $"({Lexems.JoinIntoString(", ")})";

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
