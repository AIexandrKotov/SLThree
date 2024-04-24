using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaExpression : BaseExpression
    {
        public InvokeExpression Left;
        public StatementList Right;
        public IList<string> Modificators;
        public TypenameExpression ReturnTypeHint;

        public LambdaExpression(InvokeExpression invokeExpression, StatementList statements, TypenameExpression typehint, IList<string> modificators, SourceContext context) : base(context)
        {
            Left = invokeExpression;
            Right = statements;
            ReturnTypeHint = typehint;
            Modificators = modificators;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);

            if (Method == null)
            {
                Method = new Method(
                    "$method",
                    Left.Arguments.Select(x => (x as NameExpression).Name).ToArray(),
                    Right,
                    Left.Arguments.Select(x => (x as NameExpression).TypeHint).ToArray(),
                    ReturnTypeHint,
                    null,
                    !Modificators.Contains("explicit"),
                    Modificators.Contains("recursive"),
                    false);
            }
        }

        public override string ExpressionToString() => $"{Left} => {Right}";

        public Method Method;
        public override object GetValue(ExecutionContext context)
        {
            Method.definitionplace = context.wrap;
            return Method;
        }

        public override object Clone()
        {
            return new LambdaExpression(Left.CloneCast(), Right.CloneCast(), ReturnTypeHint.CloneCast(), Modificators.ToArray().CloneArray(), SourceContext.CloneCast());
        }
    }
}
