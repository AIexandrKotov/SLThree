using SLThree.Extensions;
using SLThree.Extensions.Cloning;

using System;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

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
        public (NameExpression, TemplateMethod.ConstraintDefinition)[] GenericArguments;
        public FunctionArgument[] Arguments;
        public StatementList FunctionBody;
        public TypenameExpression ReturnTypeHint;
        public bool not_template;

        private bool @params;
        private bool not_native;
        private bool is_abstract;

        public FunctionDefinition(string[] modificators, BaseExpression name, (NameExpression, TemplateMethod.ConstraintDefinition)[] generics, FunctionArgument[] args, StatementList body, TypenameExpression typehint, ISourceContext context) : base(context)
        {
            Modificators = modificators;
            FunctionName = name;
            GenericArguments = generics;
            Arguments = args;
            FunctionBody = body;
            ReturnTypeHint = typehint;
            var many = Modificators.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
            if (many != null) throw new SyntaxError($"Repeated modifier \"{many.First()}\"", context);
            @params = modificators.Contains("params");
            not_template = !modificators.Contains("template");
            if (@params && Arguments.Length < 1) throw new LogicalError("Params method should have at least one parameter", context);
            var defaultid = args.Length;
            for (var i = 0; i < args.Length; i++)
                if (args[i].DefaultValue != null)
                {
                    defaultid = i;
                    break;
                }
            for (var i = defaultid + 1; i < args.Length + (@params ? -1 : 0); i++)
                if (args[i].DefaultValue == null)
                    throw new LogicalError("Non-default parameter between default parameters", context);

            is_abstract = Modificators.Contains("abstract");
            if (FunctionBody == null)
            {
                if (is_abstract)
                {
                    FunctionBody = new StatementList(new BaseStatement[] { new ThrowStatement(new ObjectLiteral(new AbstractInvokation(SourceContext)), context) }, context);
                }
                else throw new LogicalError("Abstract method without abstract modifier", context);
            }
            else if (is_abstract && !FunctionBody.IsAbstract()) throw new LogicalError("An abstract method shouldn't have a body", context);
            not_native = !Modificators.Contains("native");
        }

        internal DynamicMethod RebuildNative(ExecutionContext context, Method method)
        {
            return Native.Builder.Build(method, context);
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

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        public override object GetValue(ExecutionContext context)
        {
            Method method;
            if (not_template)
            {
                if (GenericArguments.Length == 0) method = new Method(
                    FunctionName == null ? Method.DefaultMethodName : CreatorContext.GetLastName(FunctionName),
                    Arguments.Select(x => x.Name.Name).ToArray(),
                    FunctionBody,
                    Arguments.Select(x => x.Name.TypeHint).ToArray(),
                    ReturnTypeHint,
                    null,
                    !Modificators.Contains("explicit"),
                    Modificators.Contains("recursive"),
                    !@params,
                    Arguments.Select(x => x.DefaultValue).Where(x => x != null).ToArray(),
                    Arguments.Select(x => x.Name.Const).ToArray());
                else
                    method = new GenericMethod(
                        FunctionName == null ? Method.DefaultMethodName : CreatorContext.GetLastName(FunctionName),
                        Arguments.Select(x => x.Name.Name).ToArray(),
                        FunctionBody,
                        Arguments.Select(x => x.Name.TypeHint).ToArray(),
                        ReturnTypeHint,
                        null,
                        !Modificators.Contains("explicit"),
                        Modificators.Contains("recursive"),
                        !@params,
                        Arguments.Select(x => x.DefaultValue).Where(x => x != null).ToArray(),
                        Arguments.Select(x => x.Name.Const).ToArray(),
                        GenericArguments.ConvertAll(x => x.Item1));
            }
            else
            {
                method = new TemplateMethod(
                FunctionName == null ? Method.DefaultMethodName : CreatorContext.GetLastName(FunctionName),
                Arguments.Select(x => x.Name.Name).ToArray(),
                FunctionBody.CloneCast(),
                Arguments.Select(x => x.Name.TypeHint.CloneCast()).ToArray(),
                ReturnTypeHint.CloneCast(),
                null,
                !Modificators.Contains("explicit"),
                Modificators.Contains("recursive"),
                !@params,
                Arguments.Select(x => x.DefaultValue.CloneCast()).Where(x => x != null).ToArray(),
                Arguments.Select(x => x.Name.Const).ToArray(),
                GenericArguments.Select(x => (x.Item1.CloneCast(), x.Item2?.GetConstraint(x.Item1.Name, context) ?? new TemplateMethod.AnyConstraint(x.Item1.SourceContext))).ToArray());
            }
            method.Abstract = is_abstract;
            if (not_native)
            {
                method.@this = context?.wrap;
                if (FunctionName != null)
                {
                    BinaryAssign.AssignToValue(context, FunctionName, method, ref counted_invoked, ref is_name_expr, ref variable_index);
                }
                return method;
            }
            else
            {
                var native = RebuildNative(context, method);
                if (FunctionName != null)
                    BinaryAssign.AssignToValue(context, FunctionName, native, ref counted_invoked, ref is_name_expr, ref variable_index);
                return native;
            }
        }

        public override object Clone()
        {
            return new FunctionDefinition(Modificators.CloneArray(), FunctionName.CloneCast(), GenericArguments.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())), Arguments.CloneArray(), Modificators.Contains("abstract") ? null : FunctionBody.CloneCast(), ReturnTypeHint.CloneCast(), SourceContext.CloneCast());
        }
    }
}
