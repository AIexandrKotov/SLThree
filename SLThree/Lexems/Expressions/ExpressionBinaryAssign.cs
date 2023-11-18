using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SLThree
{
    public class ExpressionBinaryAssign : ExpressionBinary
    {
        public override string Operator => "=";
        public ExpressionBinaryAssign(BaseLexem left, BaseLexem right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public ExpressionBinaryAssign() : base() { }
        private ExecutionContext counted_invoked;
        private bool is_namelexem;
        private int variable_index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object AssignValue(ExecutionContext context, BaseLexem Left, object right)
        {
            if (counted_invoked == context && is_namelexem)
            {
                context.LocalVariables.SetValue(variable_index, right);
                return right;
            }
            else
            {
                if (Left is NameLexem nl)
                {
                    if (right is Method mth)
                    {
                        mth = mth.CloneCast();
                        mth.Name = nl.Name;
                        mth.UpdateContextName();
                        mth.DefinitionPlace = new ExecutionContext.ContextWrap(context);
                        right = mth;
                    }
                    variable_index = context.LocalVariables.SetValue(nl.Name, right);
                    is_namelexem = true;
                    counted_invoked = context;
                    return right;
                }
                else return assignToValue(context, Left, right);
            }
            throw new OperatorError(this, Left?.GetType(), right?.GetType());
        }

        private static object assignToValue(ExecutionContext context, BaseLexem Left, object right)
        {
            if (Left is NameLexem nl)
            {
                if (right is Method mth)
                {
                    mth = mth.CloneCast();
                    mth.Name = nl.Name;
                    mth.UpdateContextName();
                    mth.DefinitionPlace = new ExecutionContext.ContextWrap(context);
                    right = mth;
                }
                context.LocalVariables.SetValue(nl.Name, right);
                return right;
            }
            else if (Left is MemberAccess memberAccess)
            {
                memberAccess.SetValue(context, ref right);
                return right;
            }
            else if (Left is IndexLexem indexLexem)
            {
                indexLexem.SetValue(context, right);
                return right;
            }
            else if (Left is CreatorTuple tuple)
            {
                tuple.SetValue(context, right);
                return right;
            }
            throw new OperatorError("=", Left?.GetType(), right?.GetType(), Left.SourceContext);
        }

        public static object AssignToValue(ExecutionContext context, BaseLexem Left, object right, ref ExecutionContext counted_invoked, ref bool is_namelexem, ref int variable_index)
        {
            if (counted_invoked == context && is_namelexem)
            {
                context.LocalVariables.SetValue(variable_index, right);
                return right;
            }

            if (Left is NameLexem nl)
            {
                if (right is Method mth)
                {
                    mth = mth.CloneCast();
                    mth.Name = nl.Name;
                    mth.UpdateContextName();
                    mth.DefinitionPlace = new ExecutionContext.ContextWrap(context);
                    right = mth;
                }
                variable_index = context.LocalVariables.SetValue(nl.Name, right);
                is_namelexem = true;
                counted_invoked = context;
                return right;
            }
            else if (Left is MemberAccess memberAccess)
            {
                memberAccess.SetValue(context, ref right);
                return right;
            }
            else if (Left is IndexLexem indexLexem)
            {
                indexLexem.SetValue(context, right);
                return right;
            }
            else if (Left is CreatorTuple tuple)
            {
                tuple.SetValue(context, right);
                return right;
            }
            throw new OperatorError("=", Left?.GetType(), right?.GetType(), Left.SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return AssignValue(context, Left, Right.GetValue(context));
        }

        public override object Clone()
        {
            return new ExpressionBinaryAssign(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
