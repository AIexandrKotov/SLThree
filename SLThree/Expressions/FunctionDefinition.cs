using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class FunctionDefinition : BaseExpression
    {
        /*
        ---Function defines---
        [MODS [Name]]<T>(args)[:TRet] {}
        [MODS [Name]]<T>(args)[:TRet] => expr
        [MODS [Name]](args)[:TRet] {}
        [MODS [Name]](args)[:TRet] => expr
        arg[:TRet] => {}
        arg[:TRet] => expr
        */

        public string[] Modificators;
        public BaseExpression FunctionName;
        public NameExpression[] GenericArguments;
        public NameExpression[] Arguments;
        public StatementList FunctionBody;
        public TypenameExpression ReturnTypeHint;

        public FunctionDefinition(string[] modificators, BaseExpression name, NameExpression[] generics, NameExpression[] args, StatementList body, TypenameExpression typehint, SourceContext context) : base(context)
        {
            Modificators = modificators;
            FunctionName = name;
            GenericArguments = generics;
            Arguments = args;
            FunctionBody = body;
            ReturnTypeHint = typehint;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);

            if (Method == null)
            {
                if (GenericArguments.Length == 0) Method = new Method(
                    FunctionName == null ? "$method" : CreatorContext.GetLastName(FunctionName),
                    Arguments.Select(x => x.Name).ToArray(),
                    FunctionBody,
                    Arguments.Select(x => x.TypeHint).ToArray(),
                    ReturnTypeHint,
                    null,
                    !Modificators.Contains("explicit"),
                    Modificators.Contains("recursive"));
                else Method = new GenericMethod(
                    FunctionName == null ? "$method" : CreatorContext.GetLastName(FunctionName),
                    Arguments.Select(x => x.Name).ToArray(),
                    FunctionBody,
                    Arguments.Select(x => x.TypeHint).ToArray(),
                    ReturnTypeHint,
                    null,
                    !Modificators.Contains("explicit"),
                    Modificators.Contains("recursive"),
                    GenericArguments);
            }
        }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();

            sb.Append(Modificators.JoinIntoString(" "));
            if (FunctionName != null)
            {
                if (Modificators.Length > 0) sb.Append(" ");
                sb.Append($"{FunctionName}");
            }
            if (GenericArguments.Length > 0) sb.Append($"<{GenericArguments.JoinIntoString(", ")}>");
            sb.Append($"({Arguments.JoinIntoString(", ")})");
            if (ReturnTypeHint != null)
                sb.Append($": {ReturnTypeHint}");
            if (FunctionBody.Statements.Length == 1 && FunctionBody.Statements[0] is ReturnStatement statement && !statement.VoidReturn)
                sb.Append($" => {statement.Expression}");
            else sb.Append($"{{{FunctionBody}}}");

            return sb.ToString();
        }

        public Method Method;
        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        public override object GetValue(ExecutionContext context)
        {
            Method.definitionplace = context.wrap;
            if (FunctionName != null)
                BinaryAssign.AssignToValue(context, FunctionName, Method, ref counted_invoked, ref is_name_expr, ref variable_index);
            return Method;
        }

        public override object Clone()
        {
            return new FunctionDefinition(Modificators.CloneArray(), FunctionName.CloneCast(), GenericArguments.CloneArray(), Arguments.CloneArray(), FunctionBody.CloneCast(), ReturnTypeHint.CloneCast(), SourceContext.CloneCast());
        }
    }
}
