using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class LambdaGenericExpression : BaseExpression
    {
        public InvokeExpression Left;
        public StatementList Right;
        public NameExpression[] Generics;
        public IList<string> Modificators;
        public TypenameExpression ReturnTypeHint;

        public LambdaGenericExpression(InvokeExpression invokeExpression, StatementList statements, TypenameExpression typehint, IList<string> modificators, NameExpression[] generics, SourceContext context) : base(context)
        {
            Left = invokeExpression;
            Right = statements;
            Generics = generics;
            ReturnTypeHint = typehint;
            Modificators = modificators;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);

            if (Method == null)
            {
                Method = new GenericMethod(
                    "$method",
                    Left.Arguments.Select(x => (x as NameExpression).Name).ToArray(),
                    Right,
                    Left.Arguments.Select(x => (x as NameExpression).TypeHint).ToArray(),
                    ReturnTypeHint,
                    null,
                    !Modificators.Contains("explicit"),
                    Modificators.Contains("recursive"),
                    false,
                    Generics);
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
            return new LambdaGenericExpression(Left.CloneCast(), Right.CloneCast(), ReturnTypeHint.CloneCast(), Modificators.ToArray().CloneArray(), Generics.CloneArray(), SourceContext.CloneCast());
        }
    }
}
