using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaExpression : BaseExpression
    {
        public InvokeExpression Left { get; set; }
        public StatementListStatement Right { get; set; }
        public IList<string> Modificators { get; set; }

        public LambdaExpression(InvokeExpression invokeExpression, StatementListStatement statements, IList<string> modificators, SourceContext context) : base(context)
        {
            Left = invokeExpression;
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
                        Name = "$method",
                        ParamNames = Left.Arguments.Select(x => (x as NameExpression).Name).ToArray(),
                        Statements = Right,
                        imp = Modificators.Contains("implicit"),
                    };
                }
                else
                {
                    Method = new Method()
                    {
                        Name = "$method",
                        ParamNames = Left.Arguments.Select(x => (x as NameExpression).Name).ToArray(),
                        Statements = Right,
                        imp = Modificators.Contains("implicit"),
                    };
                }
            }
        }

        public override string ExpressionToString() => $"{Left} => {Right}";

        public Method Method;
        public override object GetValue(ExecutionContext context)
        {
            Method.DefinitionPlace = context.wrap;
            return Method;
        }

        public override object Clone()
        {
            return new LambdaExpression(Left.CloneCast(), Right.CloneCast(), Modificators.ToArray().CloneArray(), SourceContext.CloneCast());
        }
    }
}
