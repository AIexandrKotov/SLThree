using SLThree.Extensions;
using SLThree.sys;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SLThree.Native
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
            else
            {
                throw new NotSupportedException($"{expression.GetType().GetTypeString()} is not supported for JIT.");
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
                var vi = VariablesMap[assign.Left as NameExpression];/*
                if (vi.Type != typeof(int))
                {
                    if (vi.Type == typeof(long) || vi.Type == typeof(ulong)) IL.Emit(OpCodes.Conv_I8); 
                }*/
                vi.EmitSet(IL);
                if (Executables.Count > 0 && !(Executables.LastOrDefault(x => x is BaseStatement) is ExpressionStatement)) IL.Emit(OpCodes.Dup);
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
                    case BinaryUnequals _:
                        IL.Emit(OpCodes.Ceq);
                        IL.Emit(OpCodes.Ldc_I4_0);
                        IL.Emit(OpCodes.Ceq);
                        break;
                    case BinaryLessThanEquals _:
                        IL.Emit(OpCodes.Cgt);
                        IL.Emit(OpCodes.Ldc_I4_0);
                        IL.Emit(OpCodes.Ceq);
                        break;
                    case BinaryGreaterThanEquals _:
                        IL.Emit(OpCodes.Clt);
                        IL.Emit(OpCodes.Ldc_I4_0);
                        IL.Emit(OpCodes.Ceq);
                        break;
                    case BinaryGreaterThan _: IL.Emit(OpCodes.Cgt); break;
                    case BinaryLessThan _: IL.Emit(OpCodes.Clt); break;
                    default:
                        throw new NotSupportedException($"{expression.GetType().GetTypeString()} is not supported for JIT.");
                }
                //if (Executables.Count > 0 && Executables.LastOrDefault(x => x is BaseStatement) is ExpressionStatement) IL.Emit(OpCodes.Pop);
            }
        }

        public void VisitLiteral(int literal)
        {
            switch (literal)
            {
                case -1:
                    IL.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    IL.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    IL.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    IL.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    IL.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    IL.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    IL.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    IL.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    IL.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    IL.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    IL.Emit(OpCodes.Ldc_I4, literal);
                    return;
            }
        }

        public static void EmitLoadStaticFieldOrConst(FieldInfo fieldInfo, ILGenerator il)
        {
            if (fieldInfo.Attributes.HasFlag(FieldAttributes.HasDefault) && fieldInfo.Attributes.HasFlag(FieldAttributes.Literal))
            {
                var value = fieldInfo.GetRawConstantValue();
                if (value is Enum) value = value.CastToType(Enum.GetUnderlyingType(fieldInfo.FieldType));
                switch (value)
                {
                    case long i64:
                        if (i64 >= int.MinValue && i64 <= int.MaxValue)
                        {
                            il.Emit(OpCodes.Ldc_I4, (int)i64);
                            il.Emit(OpCodes.Conv_I8);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldc_I8, i64);
                        }
                        break;
                    case ulong u64:
                        if (u64 >= 0 && u64 <= int.MaxValue)
                        {
                            il.Emit(OpCodes.Ldc_I4, (int)u64);
                            il.Emit(OpCodes.Conv_I8);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldc_I8, unchecked((long)u64));
                        }
                        break;
                    case int i32:
                        il.Emit(OpCodes.Ldc_I4, i32);
                        break;
                    case uint u32:
                        il.Emit(OpCodes.Ldc_I4, unchecked((int)u32));
                        break;
                    case float f32:
                        il.Emit(OpCodes.Ldc_R4, f32);
                        break;
                    case double f64:
                        il.Emit(OpCodes.Ldc_R8, f64);
                        break;
                    case short i:
                        il.Emit(OpCodes.Ldc_I4, (int)i);
                        il.Emit(OpCodes.Conv_I2);
                        break;
                    case ushort i:
                        il.Emit(OpCodes.Ldc_I4, (int)i);
                        il.Emit(OpCodes.Conv_I2);
                        break;
                    case sbyte i:
                        il.Emit(OpCodes.Ldc_I4, (int)i);
                        il.Emit(OpCodes.Conv_I1);
                        break;
                    case byte i:
                        il.Emit(OpCodes.Ldc_I4, (int)i);
                        il.Emit(OpCodes.Conv_I1);
                        break;
                    case bool b:
                        if (b)
                            il.Emit(OpCodes.Ldc_I4_1);
                        else
                            il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Conv_I1);
                        break;
                    case string s:
                        il.Emit(OpCodes.Ldstr, s);
                        break;
                    default:
                        throw new NotSupportedException($"{fieldInfo.FieldType} of constant not supported");
                }
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, fieldInfo);
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
                            VisitLiteral((int)value);
                            IL.Emit(OpCodes.Conv_I8);
                        }
                        else IL.Emit(OpCodes.Ldc_I8, value);
                    }
                    break;
                case ULongLiteral u64:
                    {
                        var value = (ulong)u64.Value;
                        if (value >= uint.MinValue && value <= uint.MaxValue)
                        {
                            VisitLiteral((int)(uint)value);
                            IL.Emit(OpCodes.Conv_I8);
                        }
                        else IL.Emit(OpCodes.Ldc_I8, (long)value);
                    }
                    IL.Emit(OpCodes.Ldc_I8, (long)u64.Value);
                    break;
                case IntLiteral i32:
                    VisitLiteral((int)i32.Value);
                    break;
                case UIntLiteral u32:
                    VisitLiteral((int)(uint)u32.Value);
                    break;
                case FloatLiteral f32:
                    IL.Emit(OpCodes.Ldc_R4, (float)f32.Value);
                    break;
                case DoubleLiteral f64:
                    IL.Emit(OpCodes.Ldc_R8, (double)f64.Value);
                    break;
                case BoolLiteral @bool:
                    IL.Emit((bool)@bool.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    break;
            }
        }

        public override void VisitStatement(ExpressionStatement statement)
        {
            base.VisitStatement(statement);
        }

        public bool IsLastStatement = false;

        public Stack<(Label, Label)> LoopCondEnds = new Stack<(Label, Label)>();

        public override void Visit(Method method)
        {
            var count = method.Statements.Statements.Length;
            if (count == 0)
            {
                IL.Emit(OpCodes.Ret);
                return;
            }
            var current = 0;
            foreach (var x in method.Statements.Statements)
            {
                current++;
                IsLastStatement = current == count;
                VisitStatement(x);
            }
        }

        /// <summary>
        /// Создаёт переход на метку если условие не выполняется
        /// </summary>
        public void MakeBRNotCondition(BaseExpression expression, Label label)
        {
            switch (expression)
            {
                case BinaryLessThan bge:
                    VisitExpression(bge.Left);
                    VisitExpression(bge.Right);
                    IL.Emit(OpCodes.Bge_S, label);
                    break;
                case BinaryLessThanEquals bgt:
                    VisitExpression(bgt.Left);
                    VisitExpression(bgt.Right);
                    IL.Emit(OpCodes.Bgt_S, label);
                    break;
                case BinaryGreaterThan ble:
                    VisitExpression(ble.Left);
                    VisitExpression(ble.Right);
                    IL.Emit(OpCodes.Ble_S, label);
                    break;
                case BinaryGreaterThanEquals blt:
                    VisitExpression(blt.Left);
                    VisitExpression(blt.Right);
                    IL.Emit(OpCodes.Blt_S, label);
                    break;
                case BinaryUnequals beq:
                    VisitExpression(beq.Left);
                    VisitExpression(beq.Right);
                    IL.Emit(OpCodes.Beq_S, label);
                    break;
                default:
                    VisitExpression(expression);
                    IL.Emit(OpCodes.Brfalse_S, label);
                    break;
            }
        }
        /// <summary>
        /// Создаёт переход на метку если условие выполняется
        /// </summary>
        public void MakeBRCondition(BaseExpression expression, Label label)
        {
            switch (expression)
            {
                case BinaryLessThan blt:
                    VisitExpression(blt.Left);
                    VisitExpression(blt.Right);
                    IL.Emit(OpCodes.Blt_S, label);
                    break;
                case BinaryLessThanEquals ble:
                    VisitExpression(ble.Left);
                    VisitExpression(ble.Right);
                    IL.Emit(OpCodes.Ble_S, label);
                    break;
                case BinaryGreaterThan bgt:
                    VisitExpression(bgt.Left);
                    VisitExpression(bgt.Right);
                    IL.Emit(OpCodes.Bgt_S, label);
                    break;
                case BinaryGreaterThanEquals bge:
                    VisitExpression(bge.Left);
                    VisitExpression(bge.Right);
                    IL.Emit(OpCodes.Bge_S, label);
                    break;
                case BinaryUnequals beq:
                    VisitExpression(beq.Left);
                    VisitExpression(beq.Right);
                    IL.Emit(OpCodes.Beq_S, label);
                    break;
                default:
                    VisitExpression(expression);
                    IL.Emit(OpCodes.Brtrue_S, label);
                    break;
            }
        }

        public override void VisitExpression(ConditionExpression statement)
        {
            var ifbody = statement.IfBody;
            var elsebody = statement.ElseBody;

            if (elsebody.Length == 0)
            {
                var label = IL.DefineLabel();
                MakeBRNotCondition(statement.Condition, label);

                foreach (var x in ifbody)
                    VisitStatement(x);
                IL.MarkLabel(label);
            }
            else
            {
                var not_is_last = !IsLastStatement;
                var labelelse = IL.DefineLabel();
                var labelexit = IL.DefineLabel();

                MakeBRNotCondition(statement.Condition, labelelse);

                foreach (var x in ifbody)
                    VisitStatement(x);

                IL.Emit(OpCodes.Br_S, labelexit);

                IL.MarkLabel(labelelse);

                foreach (var x in elsebody)
                    VisitStatement(x);

                IL.MarkLabel(labelexit);
                if (!not_is_last) IL.Emit(OpCodes.Nop);
            }
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            var not_is_last = !IsLastStatement;
            var label_condition = IL.DefineLabel();
            var label_end = IL.DefineLabel();
            LoopCondEnds.Push((label_condition, label_end));

            IL.MarkLabel(label_condition);
            MakeBRNotCondition(statement.Condition, label_end);
            foreach (var x in statement.LoopBody)
                VisitStatement(x);
            IL.Emit(OpCodes.Br_S, label_condition);

            IL.MarkLabel(label_end);
            if (!not_is_last) IL.Emit(OpCodes.Nop);
            LoopCondEnds.Pop();
        }

        public override void VisitStatement(BreakStatement statement)
        {
            if (LoopCondEnds.Count == 0) throw new NotSupportedException("`break;` out of loop");
            IL.Emit(OpCodes.Br_S, LoopCondEnds.Peek().Item2);
        }

        public override void VisitStatement(ContinueStatement statement)
        {
            if (LoopCondEnds.Count == 0) throw new NotSupportedException("`continue;` out of loop");
            IL.Emit(OpCodes.Br_S, LoopCondEnds.Peek().Item1);
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            if (statement.Expression != null)
                VisitExpression(statement.Expression);
            IL.Emit(OpCodes.Ret);
        }

        public NETGenerator(Method method, ExecutionContext context, ILGenerator generator)
        {
            (Variables, VariablesMap) = NameCollector.Collect(method, context);
            IL = generator;
            var paramid = 1;
            foreach (var x in Variables)
            {
                if (x.NameType == NameType.Local)
                {
                    var lb = IL.DeclareLocal(x.Type);
#if NETFRAMEWORK
                    lb.SetLocalSymInfo(x.Name);
#endif
                    index++;
                }
            }
        }
        public NETGenerator(Method method, ExecutionContext context, MethodBuilder mb, ILGenerator generator)
        {
            (Variables, VariablesMap) = NameCollector.Collect(method, context);
            IL = generator;
            var paramid = 1;
            foreach (var x in Variables)
            {
                if (x.NameType == NameType.Local)
                {
                    var lb = IL.DeclareLocal(x.Type);
#if NETFRAMEWORK
                    lb.SetLocalSymInfo(x.Name);
#endif
                    index++;
                }
                else if (x.NameType == NameType.Parameter)
                {
                    mb.DefineParameter(paramid, ParameterAttributes.None, x.Name);
                    paramid++;
                }
            }
        }
    }
}
