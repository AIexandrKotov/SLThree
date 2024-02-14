using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SLThree.JIT
{
    public class NETGenerator : AbstractVisitor
    {
        public ILGenerator IL;
        public List<AbstractNameInfo> Variables = new List<AbstractNameInfo>();
        public Dictionary<NameExpression, AbstractNameInfo> VariablesMap = new Dictionary<NameExpression, AbstractNameInfo>();
        private int index = 0;

        public override void VisitExpression(NameExpression expression)
        {
            if (VariablesMap.TryGetValue(expression, out var vi))
            {
                vi.EmitGet(IL);
            }
        }

        public override void VisitExpression(UnaryOperator expression)
        {
            if (expression is UnaryAdd) return;
            VisitExpression(expression.Left);
            switch (expression)
            {
                case UnaryRem _: IL.Emit(OpCodes.Neg); break;
                case UnaryBitNot _: IL.Emit(OpCodes.Not); break;
                case UnaryNot _:
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq); 
                    break;
                default:
                    throw new NotSupportedException($"{expression.GetType().GetTypeString()} is not supported for JIT.");
            }
        }

        public override void VisitExpression(BinaryOperator expression)
        {
            if (expression is BinaryAssign assign)
            {
                VisitExpression(assign.Right);
                VariablesMap[assign.Left as NameExpression].EmitSet(IL);
            }
            else
            {
                VisitExpression(expression.Left);
                VisitExpression(expression.Right);
                switch (expression)
                {
                    case BinaryAdd _: IL.Emit(OpCodes.Add); break;
                    case BinaryRem _: IL.Emit(OpCodes.Sub); break;
                    case BinaryMultiply _: IL.Emit(OpCodes.Mul); break;
                    case BinaryDivide _: IL.Emit(OpCodes.Div); break;
                    case BinaryMod _: IL.Emit(OpCodes.Rem); break;
                    case BinaryBitXor _: IL.Emit(OpCodes.Xor); break;
                    case BinaryBitAnd _: IL.Emit(OpCodes.And); break;
                    case BinaryBitOr _: IL.Emit(OpCodes.Or); break;
                    case BinaryAnd _: IL.Emit(OpCodes.And); break;
                    case BinaryOr _: IL.Emit(OpCodes.Or); break;
                    case BinaryEquals _: IL.Emit(OpCodes.Ceq); break;
                    default:
                        throw new NotSupportedException($"{expression.GetType().GetTypeString()} is not supported for JIT.");
                }
            }
        }

        public override void VisitExpression(Literal expression)
        {
            switch (expression)
            {
                case StringLiteral str:
                    IL.Emit(OpCodes.Ldstr, (string)str.Value);
                    break;
                case LongLiteral i64:
                    {
                        var value = (long)i64.Value;
                        if (value >= int.MinValue && value <= int.MaxValue)
                        {
                            IL.Emit(OpCodes.Ldc_I4, (int)value);
                            IL.Emit(OpCodes.Conv_I8);
                        }
                        else IL.Emit(OpCodes.Ldc_I8, value);
                    }
                    break;
                case ULongLiteral u64:
                    IL.Emit(OpCodes.Ldc_I8, (ulong)u64.Value);
                    break;
            }
        }

        public override void VisitStatement(ExpressionStatement statement)
        {
            base.VisitStatement(statement);
            IL.Emit(OpCodes.Pop);
        }

        public Stack<bool> IsLastStatement = new Stack<bool>();

        public override void VisitStatement(ConditionStatement statement)
        {
            var ifbody = statement.IfBody;
            var elsebody = statement.ElseBody;

            if (elsebody.Length == 0)
            {
                VisitExpression(statement.Condition);
                var label = IL.DefineLabel();
                IL.Emit(OpCodes.Brfalse_S, label);

                foreach (var x in ifbody)
                    VisitStatement(x);
                IL.MarkLabel(label);
            }
            else
            {
                var not_is_last = !(IsLastStatement.Count == 0 || IsLastStatement.Peek());
                var labelelse = IL.DefineLabel();
                var labelexit = default(Label);
                if (not_is_last)
                    labelexit = IL.DefineLabel();
                IL.DeclareLocal(typeof(bool));
                var index = this.index;
                this.index++;

                VisitExpression(statement.Condition);
                AbstractNameInfo.SetLocal(IL, index);

                AbstractNameInfo.LoadLocal(IL, index);
                IL.Emit(OpCodes.Brfalse_S, labelelse);

                foreach (var x in ifbody)
                    VisitStatement(x);

                if (not_is_last)
                {
                    IL.Emit(OpCodes.Br_S, labelexit);
                }

                IL.MarkLabel(labelelse);
                foreach (var x in elsebody)
                    VisitStatement(x);

                if (not_is_last)
                {
                    IL.MarkLabel(labelexit);
                }
            }
        }

        public override void VisitStatement(StatementList statement)
        {
            var count = statement.Statements.Length;
            var current = 0;
            foreach (var x in statement.Statements)
            {
                current++;
                IsLastStatement.Push(current == count);
                VisitStatement(x);
                IsLastStatement.Pop();
            }
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            if (statement.Expression != null)
                VisitExpression(statement.Expression);
            IL.Emit(OpCodes.Ret);
        }

        public NETGenerator(Method method, ExecutionContext context, ILGenerator generator)
        {
            (Variables, VariablesMap) = NameCollector.Collect(method);
            IL = generator;
            foreach (var x in Variables)
            {
                if (x.NameType == NameType.Local)
                {
                    IL.DeclareLocal(x.Type);
                    index++;
                }
            }
        }
    }
}
