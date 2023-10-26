using Pegasus.Common;
using SLThree.Extensions;
using System.Linq;

namespace SLThree
{
    public class LambdaLexem : BoxSupportedLexem
    {
        public InvokeLexem Left { get; set; }
        public StatementListStatement Right { get; set; }

        public LambdaLexem(InvokeLexem invokeLexem, StatementListStatement statements, Cursor cursor) : base(cursor)
        {
            Left = invokeLexem;
            Right = statements;
        }

        public override string ToString() => $"{Left} => {Right}";

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            return ref new Method() { Name = "anon_method", ParamNames = Left.Arguments.Select(x => (x as NameLexem).Name).ToArray(), Statements = Right }.ToSpeedy(ref reference);
        }


        public string Operator => "=>";
    }
}
