using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class ChanceChooseLexem : BaseLexem
    {
        public static readonly Random Random = new Random();

        public IList<(BaseLexem, BaseLexem)> Chooser { get; set; }

        public ChanceChooseLexem(IList<(BaseLexem, BaseLexem)> elements, SourceContext context) : base(context)
        {
            Chooser = elements;
        }
        public ChanceChooseLexem(IList<(BaseLexem, BaseLexem)> elements, Cursor cursor) : this(elements, new SourceContext(cursor))
        {
            Chooser = elements;
        }

        public ChanceChooser<object> GetChooser(ExecutionContext context)
        {
            return new ChanceChooser<object>(Chooser.Select(
                x => (x.Item1.GetValue(context), 
                context.fimp ? (double)x.Item2.GetValue(context) 
                : (double)x.Item2.GetValue(context).CastToType(typeof(double)))).ToArray());
        }

        public override object GetValue(ExecutionContext context)
        {
            var doubles = Chooser.Select(x => context.fimp ? (double)x.Item2.GetValue(context) : (double)x.Item2.GetValue(context).CastToType(typeof(double))).ToArray();
            var doubles_sum = doubles.Sum();


            var t = Random.NextDouble();
            var sum = 0.0;
            for (var i = 0; i < Chooser.Count; i++)
            {
                sum += doubles[i] / doubles_sum;
                if (sum >= t) return Chooser[i].Item1.GetValue(context);
            }
            return Chooser[Chooser.Count - 1].Item1.GetValue(context);
        }

        public override string ToString() => Chooser.Select(x => $"{x.Item1}: {x.Item2}").JoinIntoString(" \\ ");

        public override object Clone()
        {
            return new ChanceChooseLexem(Chooser, SourceContext);
        }
    }
}
