using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaExpression : BaseExpression
    {
        public InvokeExpression Left;
        public StatementListStatement Right;
        public IList<string> Modificators;
        public TypenameExpression ReturnTypeHint;

        public LambdaExpression(InvokeExpression invokeExpression, StatementListStatement statements, TypenameExpression typehint, IList<string> modificators, SourceContext context) : base(context)
        {
            Left = invokeExpression;
            Right = statements;
            ReturnTypeHint = typehint;
            Modificators = modificators;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);

            if (Method == null)
            {
                Method = new Method()
                {
                    Name = "$method",
                    ParamNames = Left.Arguments.Select(x => (x as NameExpression).Name).ToArray(),
                    ParamTypes = Left.Arguments.Select(x => (x as NameExpression).TypeHint).ToArray(),
                    ReturnType = ReturnTypeHint,
                    Statements = Right,
                    imp = Modificators.Contains("implicit"),
                };
                if (Modificators.Contains("recursive")) Method = Method.MakeRecursive();
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
            return new LambdaExpression(Left.CloneCast(), Right.CloneCast(), ReturnTypeHint.CloneCast(), Modificators.ToArray().CloneArray(), SourceContext.CloneCast());
        }
    }
}
