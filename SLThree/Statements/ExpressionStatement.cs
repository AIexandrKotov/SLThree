using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class ExpressionStatement : BaseStatement
    {
        public BaseLexem Lexem;

        public ExpressionStatement(BaseLexem lexem, SourceContext context) : base(context)
        {
            Lexem = lexem;
        }
        public ExpressionStatement(BaseLexem lexem, Cursor cursor) : base(cursor)
        {
            Lexem = lexem;
        }

        public override string ToString() => $"{Lexem}";
        public override object GetValue(ExecutionContext context) => Lexem.GetValue(context);

        public override object Clone()
        {
            return new ExpressionStatement(Lexem.CloneCast(), SourceContext.CloneCast());
        }
    }
}
