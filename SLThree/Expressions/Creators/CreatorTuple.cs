using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class CreatorTuple : BaseExpression
    {
        public BaseExpression[] Expressions;
        public (ExecutionContext, bool, int)[] Caches;
        
        public CreatorTuple(BaseExpression[] expressions, SourceContext context) : base(context)
        {
            Expressions = expressions;
            Caches = new (ExecutionContext, bool, int)[expressions.Length];
        }

        public void SetValue(ExecutionContext context, object right)
        {
            if (right is ITuple tuple)
            {
                var min = Math.Min(Expressions.Length, tuple.Length);
                for (var i = 0; i < min; i++)
                    ExpressionBinaryAssign.AssignToValue(context, Expressions[i], tuple[i], ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
            }
            else throw new RuntimeError("Right value must be tuple", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return Create(Expressions.ConvertAll(x => x.GetValue(context)));
        }

        private static Type generic_vt8 = typeof(ValueTuple<,,,,,,,>);

        private static Type[] objs7 = Enumerable.Repeat(typeof(object), 7).ToArray();
        private static Type[] get_objs8(Type next) =>
            objs7.Append(next).ToArray();

        public static ITuple Create(object[] objs, int index = 0)
        {
            switch (objs.Length - index)
            {
                case 0: throw new ArgumentException("Zero length array in objs");
                case 1: return new ValueTuple<object>(objs[index]);
                case 2: return new ValueTuple<object, object>(objs[index], objs[index + 1]);
                case 3: return new ValueTuple<object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2]);
                case 4: return new ValueTuple<object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3]);
                case 5: return new ValueTuple<object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4]);
                case 6: return new ValueTuple<object, object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5]);
                case 7: return new ValueTuple<object, object, object, object, object, object, object>(objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5], objs[index + 6]);
                default:
                    {
                        var rest = Create(objs, index + 7);
                        return (ITuple)Activator.CreateInstance(generic_vt8.MakeGenericType(get_objs8(rest.GetType())),
                            new object[8]
                            { objs[index + 0], objs[index + 1], objs[index + 2], objs[index + 3], objs[index + 4], objs[index + 5], objs[index + 6], rest });
                    }
            }
        }
        public static object[] ToArray(ITuple tuple)
        {
            var ret = new object[tuple.Length];
            for (var i = 0; i < tuple.Length; i++)
                ret[i] = tuple[i];
            return ret;
        }
        public override string ExpressionToString() => $"({Expressions.JoinIntoString(", ")})";

        public override object Clone()
        {
            return new CreatorTuple(Expressions.CloneArray(), SourceContext.CloneCast());
        }
    }
}
