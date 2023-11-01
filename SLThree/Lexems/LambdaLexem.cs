using Pegasus.Common;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaLexem : BaseLexem
    {
        public InvokeLexem Left { get; set; }
        public StatementListStatement Right { get; set; }
        public IList<string> Modificators { get; set; }

        public LambdaLexem(InvokeLexem invokeLexem, StatementListStatement statements, IList<string> modificators, Cursor cursor) : base(cursor)
        {
            Left = invokeLexem;
            Right = statements;
            Modificators = modificators;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", cursor);
        }

        public override string ToString() => $"{Left} => {Right}";

        private Method method;
        public override object GetValue(ExecutionContext context)
        {
            if (method == null)
            {
                method = new Method()
                {
                    Name = "anon_method",
                    ParamNames = Left.Arguments.Select(x => (x as NameLexem).Name).ToArray(),
                    Statements = Right,
                    imp = Modificators.Contains("implicit")
                };
            }
            return method;
        }
    }
}
