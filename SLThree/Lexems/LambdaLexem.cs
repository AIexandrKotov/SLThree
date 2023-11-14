using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaLexem : BaseLexem
    {
        public InvokeLexem Left { get; set; }
        public StatementListStatement Right { get; set; }
        public IList<string> Modificators { get; set; }

        public LambdaLexem(InvokeLexem invokeLexem, StatementListStatement statements, IList<string> modificators, SourceContext context) : base(context)
        {
            Left = invokeLexem;
            Right = statements;
            Modificators = modificators;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);

            if (Method == null)
            {
                if (Modificators.Contains("recursive"))
                {
                    Method = new RecursiveMethod()
                    {
                        Name = "anon_method",
                        ParamNames = Left.Arguments.Select(x => (x as NameLexem).Name).ToArray(),
                        Statements = Right,
                        imp = Modificators.Contains("implicit"),
                    };
                }
                else
                {
                    Method = new Method()
                    {
                        Name = "anon_method",
                        ParamNames = Left.Arguments.Select(x => (x as NameLexem).Name).ToArray(),
                        Statements = Right,
                        imp = Modificators.Contains("implicit"),
                    };
                }
            }
        }

        public override string LexemToString() => $"{Left} => {Right}";

        public Method Method;
        public override object GetValue(ExecutionContext context)
        {
            return Method;
        }

        public override object Clone()
        {
            return new LambdaLexem(Left.CloneCast(), Right.CloneCast(), Modificators.ToArray().CloneArray(), SourceContext.CloneCast());
        }
    }
}
