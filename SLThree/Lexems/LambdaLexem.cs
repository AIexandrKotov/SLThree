using Pegasus.Common;
using System.Linq;

namespace SLThree
{
    public class LambdaLexem : BaseLexem
    {
        public InvokeLexem Left { get; set; }
        public StatementListStatement Right { get; set; }

        public LambdaLexem(InvokeLexem invokeLexem, StatementListStatement statements, Cursor cursor) : base(cursor)
        {
            Left = invokeLexem;
            Right = statements;
        }

        public override string ToString() => $"{Left} => {Right}";

        public override object GetValue(ExecutionContext context)
        {
            return new Method() { Name = "anon_method", ParamNames = Left.Arguments.Select(x => (x as NameLexem).Name).ToArray(), Statements = Right };
        }

        public string Operator => "=>";
    }
}
