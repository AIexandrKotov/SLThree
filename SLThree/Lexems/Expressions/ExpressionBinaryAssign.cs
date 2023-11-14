using Pegasus.Common;
using SLThree.Extensions.Cloning;
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
            }
            throw new OperatorError(this, Left?.GetType(), right?.GetType());
        }

        public override object GetValue(ExecutionContext context)
        {
            var right = Right.GetValue(context);
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
            }
            context.Errors.Add(new OperatorError(this, Left?.GetType(), right?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new ExpressionBinaryAssign(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
