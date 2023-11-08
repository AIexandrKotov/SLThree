using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Tools.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class EqualchanceChooseLexem : BaseLexem
    {
        public static readonly Random Random = new Random();

        public EqualchanceChooser<BaseLexem> Chooser { get; set; }

        public EqualchanceChooseLexem(IList<BaseLexem> elements, Cursor cursor) : base(cursor)
        {
            Chooser = new EqualchanceChooser<BaseLexem>(elements);
        }

        public EqualchanceChooser<object> GetChooser(ExecutionContext context)
        {
            return new EqualchanceChooser<object>(Chooser.Values.Select(x => x.GetValue(context)).ToArray());
        }

        public override object GetValue(ExecutionContext context)
        {
            return Chooser.Choose().GetValue(context);
        }

        public override string ToString() => Chooser.Values.JoinIntoString(" \\ ");
    }
}
