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
        private (ExecutionContext, bool, int)[] Caches;

        public CreatorTuple(BaseExpression[] expressions, ISourceContext context) : base(context)
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
                    BinaryAssign.AssignToValue(context, Expressions[i], tuple[i], ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
            }
            else throw new IsNotTuple(SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return Create(Expressions.ConvertAll(x => x.GetValue(context)));
        }

        private static Type[] generic_vt = new Type[]
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>)
        };
        private static Type generic_vt8 = typeof(ValueTuple<,,,,,,,>);
        public static ITuple Create(object[] objs, int index = 0)
        {
            switch (objs.Length - index)
            {
                case 0: throw new ArgumentException("Zero length array in objs");
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return (ITuple)Activator.CreateInstance(
                            generic_vt[objs.Length - index - 1].MakeGenericType(
                                objs
                                    .Skip(index)
                                    .Select(x => x?.GetType() ?? typeof(object))
                                    .ToArray()),
                            objs
                                .Skip(index)
                                .ToArray());
                default:
                    {
                        var rest = Create(objs, index + 7);
                        return (ITuple)Activator.CreateInstance(
                            generic_vt8.MakeGenericType(
                                objs
                                    .Skip(index)
                                    .Take(7)
                                    .Select(x => x?.GetType() ?? typeof(object))
                                    .Append(rest.GetType())
                                    .ToArray()),
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
