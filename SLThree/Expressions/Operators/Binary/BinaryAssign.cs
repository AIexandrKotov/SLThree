using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class BinaryAssign : BinaryOperator
    {
        public override string Operator => "=";
        public BinaryAssign(BaseExpression left, BaseExpression right, ISourceContext context) : base(left, right, context) { }
        public BinaryAssign() : base() { }
        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object AssignValue(ExecutionContext context, BaseExpression Left, object right)
        {
            if (counted_invoked == context && is_name_expr)
            {
                context.LocalVariables.SetValue(variable_index, right);
                return right;
            }
            else
            {
                if (Left is NameExpression nl)
                {
                    if (right is Method mth)
                    {
                        if (mth.Binded) right = mth;
                        else
                        {
                            mth.Name = nl.Name;
                            mth.UpdateContextName();
                            mth.@this = new ContextWrap(context);
                            mth.Binded = true;
                            right = mth;
                        }
                    }
                    variable_index = context.LocalVariables.SetValue(nl.Name, right);
                    if (nl.Const) context.LocalVariables.MakeConstant(variable_index);
                    is_name_expr = true;
                    counted_invoked = context;
                    return right;
                }
                else return InternalAssignToValue(context, Left, right);
            }
            throw new OperatorError(this, Left?.GetType(), right?.GetType());
        }

        private static object InternalAssignToValue(ExecutionContext context, BaseExpression Left, object right)
        {
            if (Left is NameExpression nl)
            {
                if (right is Method mth)
                {
                    if (mth.Binded) right = mth;
                    else
                    {
                        mth.Name = nl.Name;
                        mth.UpdateContextName();
                        mth.@this = new ContextWrap(context);
                        mth.Binded = true;
                        right = mth;
                    }
                }
                var ind = context.LocalVariables.SetValue(nl.Name, right);
                if (nl.Const) context.LocalVariables.MakeConstant(ind);
                return right;
            }
            else if (Left is MemberAccess memberAccess)
            {
                memberAccess.SetValue(context, ref right);
                return right;
            }
            else if (Left is IndexExpression indexExpression)
            {
                indexExpression.SetValue(context, right);
                return right;
            }
            else if (Left is CreatorTuple tuple)
            {
                tuple.SetValue(context, right);
                return right;
            }
            else if (Left is DereferenceExpression dereference)
            {
                return dereference.SetValue(context, right);
            }
            throw new OperatorError("=", Left?.GetType(), right?.GetType(), Left.SourceContext);
        }

        public static object AssignToValue(ExecutionContext context, BaseExpression Left, object right, ref ExecutionContext counted_invoked, ref bool is_name_expr, ref int variable_index)
        {
            if (counted_invoked == context && is_name_expr)
            {
                context.LocalVariables.SetValue(variable_index, right);
                return right;
            }

            if (Left is NameExpression nl)
            {
                if (right is Method mth)
                {
                    if (mth.Binded) right = mth;
                    else
                    {
                        mth.Name = nl.Name;
                        mth.UpdateContextName();
                        mth.@this = new ContextWrap(context);
                        mth.Binded = true;
                        right = mth;
                    }
                }
                variable_index = context.LocalVariables.SetValue(nl.Name, right);
                if (nl.Const) context.LocalVariables.MakeConstant(variable_index);
                is_name_expr = true;
                counted_invoked = context;
                return right;
            }
            else if (Left is MemberAccess)
            {
                Left.Cast<MemberAccess>().SetValue(context, ref right);
                return right;
            }
            else if (Left is IndexExpression)
            {
                Left.Cast<IndexExpression>().SetValue(context, right);
                return right;
            }
            else if (Left is CreatorTuple)
            {
                Left.Cast<CreatorTuple>().SetValue(context, right);
                return right;
            }
            else if (Left is DereferenceExpression)
            {
                Left.Cast<DereferenceExpression>().SetValue(context, right);
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
            return new BinaryAssign(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
