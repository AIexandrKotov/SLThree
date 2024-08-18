using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SLThree
{
    public class MemberAccess : BinaryOperator
    {

        public override string Operator => ".";
        public MemberAccess(BaseExpression left, BaseExpression right, SourceContext context) : base(left, right, context) { }
        public MemberAccess(BaseExpression left, BaseExpression right, bool null_conditional, SourceContext context) : base(left, right, context)
        {
            this.null_conditional = null_conditional;
        }
        public MemberAccess() : base() { }

        private FieldInfo field;
        private PropertyInfo prop;
        private Type nest_type;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        private bool counted_contextwrapcache;
        private string variable_name;

        private bool counted_contextwrapcache2;
        private bool is_unwrap;
        private bool is_super;
        private bool is_upper;
        private bool is_parent;

        public static object GetNameExprValue(ExecutionContext context, object left, NameExpression Right)
        {
            if (left != null)
            {
                if (left is ContextWrap pred)
                {
                    if (Right is NameExpression predName)
                    {
                        if (predName.Name == "super")
                        {
                            return pred.Context.super;
                        }
                        else if (predName.Name == "parent")
                        {
                            return pred.Context.parent;
                        }
                        else if (predName.Name == "upper")
                        {
                            return pred.Context.PreviousContext;
                        }
                        return pred.Context.LocalVariables.GetValue(predName.Name).Item1;
                    }
                }
                var has_access = left is ClassAccess access;
                var type = has_access ? (left as ClassAccess).Name : left.GetType();

                var field = type.GetField(Right.Name);
                if (field != null) return field.GetValue(left);
                var prop = type.GetProperty(Right.Name);
                if (prop != null) return prop.GetValue(left);
                var nest_type = type.GetNestedType(Right.Name);
                if (nest_type != null) return new ClassAccess(nest_type);
                if (left is IDictionary dict)
                    return dict[Right.Name];

                throw new NameNotFound(Right.Name, type, Right.SourceContext);
            }

            return null;
        }

        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);

            if (counted_contextwrapcache && left is ContextWrap)
            {
                if (is_super) return (left as ContextWrap).Context.super;
                else if (is_parent) return (left as ContextWrap).Context.parent;
                else if (is_upper) return (left as ContextWrap).Context.PreviousContext;
                else return (left as ContextWrap).Context.LocalVariables.GetValue(variable_name).Item1;
            }
            else if (counted_contextwrapcache2 && left is ContextWrap)
            {
                if (is_unwrap) return (left as ContextWrap).Context;
                else return (Right as InvokeExpression).GetValue((left as ContextWrap).Context, (Right as InvokeExpression).Arguments.ConvertAll(x => x.GetValue(context)));
            }

            if (left != null)
            {
                if (left is ContextWrap pred)
                {
                    if (Right is NameExpression predName)
                    {
                        if (predName.Name == "super")
                        {
                            counted_contextwrapcache = true;
                            is_super = true;
                            return pred.Context.super;
                        }
                        else if (predName.Name == "parent")
                        {
                            counted_contextwrapcache = true;
                            is_parent = true;
                            return pred.Context.parent;
                        }
                        else if (predName.Name == "upper")
                        {
                            counted_contextwrapcache = true;
                            is_upper = true;
                            return pred.Context.PreviousContext;
                        }
                        variable_name = predName.ExpressionToString().Replace(" ", "");
                        counted_contextwrapcache = true;
                        return pred.Context.LocalVariables.GetValue(variable_name).Item1;
                    }
                    else if (Right is InvokeExpression invokeExpression)
                    {
                        counted_contextwrapcache2 = true;
                        if (invokeExpression.Left?.TryCastRef<NameExpression>()?.Name == "unwrap" && invokeExpression.Arguments.Length == 0)
                        {
                            is_unwrap = true;
                            return pred.Context;
                        }
                        return invokeExpression.GetValue(pred.Context, invokeExpression.Arguments.Select(x => x.GetValue(context)).ToArray());
                    }
                    else if (Right is InvokeGenericExpression invokeGenericExpression)
                    {
                        return invokeGenericExpression.GetValue(pred.Context,
                            invokeGenericExpression.GenericArguments.ConvertAll(x => (Type)x.GetValue(context)),
                            invokeGenericExpression.Arguments.ConvertAll(x => x.GetValue(context)));
                    }
                }
                var has_access = left is ClassAccess access;
                var type = has_access ? (left as ClassAccess).Name : left.GetType();
                if (field != null) return field.GetValue(left);
                if (prop != null) return prop.GetValue(left);
                if (nest_type != null) return new ClassAccess(nest_type);

                if (Right is NameExpression nameExpression)
                {
                    field = type.GetField(nameExpression.Name);
                    if (field != null) return field.GetValue(left);
                    prop = type.GetProperty(nameExpression.Name);
                    if (prop != null) return prop.GetValue(left);
                    nest_type = type.GetNestedType(nameExpression.Name);
                    if (nest_type != null) return new ClassAccess(nest_type);
                    if (left is IDictionary dict)
                        return dict[nameExpression.Name];

                    throw new NameNotFound(nameExpression.Name, type, SourceContext);
                }
                else if (Right is InvokeExpression invokeExpression)
                {
                    return invokeExpression.GetValue(context, left);
                }
                else if (Right is InvokeGenericExpression invokeGenericExpression)
                {
                    return invokeGenericExpression.GetValue(context, left);
                }
            }
            else if (null_conditional) return null;

            throw new OperatorError(this, left?.GetType(), Right?.GetType());
        }

        private bool counted_other_context_assign;
        private string other_context_name;
        public void SetValue(ExecutionContext context, ref object value)
        {
            var left = Left.GetValue(context);

            if (counted_other_context_assign)
            {
                (left as ContextWrap).Context.LocalVariables.SetValue(other_context_name, value);
                return;
            }

            if (left != null)
            {
                if (left is ContextWrap wrap)
                {
                    context = wrap.Context;
                    //var has_access_2 = left is ClassAccess;
                    //var type_2 = has_access_2 ? (left as ClassAccess).Name : left.GetType();
                    if (Right is NameExpression nameExpression2)
                    {
                        other_context_name = Right.ExpressionToString().Replace(" ", "");
                        counted_other_context_assign = true;
                        if (value is Method mth)
                        {
                            if (mth.Binded) value = mth;
                            else
                            {
                                mth.Name = nameExpression2.Name;
                                mth.UpdateContextName();
                                mth.@this = new ContextWrap(context);
                                mth.Binded = true;
                                value = mth;
                            }
                        }
                        context.LocalVariables.SetValue(other_context_name, value);
                    }
                    return;
                }
                var has_access = left is ClassAccess;
                var type = has_access ? (left as ClassAccess).Name : left.GetType();
                if (field != null)
                {
                    if (field.IsInitOnly) throw new AssignToReadonly(Right, SourceContext);
                    field.SetValue(left, value);
                    return;
                }
                if (prop != null)
                {
                    prop.SetValue(left, value);
                    return;
                }
                if (Right is NameExpression nameExpression)
                {
                    field = type.GetField(nameExpression.Name);
                    if (field != null)
                    {
                        if (field.IsInitOnly) throw new AssignToReadonly(Right, SourceContext);
                        field.SetValue(left, value);
                        return;
                    }
                    prop = type.GetProperty(nameExpression.Name);
                    if (prop != null)
                    {
                        prop.SetValue(left, value);
                        return;
                    }
                    if (left is IDictionary dict)
                    {
                        dict[nameExpression.Name] = value;
                        return;
                    }
                    throw new NameNotFound(nameExpression.Name, type, SourceContext);
                }
            }
            else if (null_conditional) return;

            throw new OperatorError(this, left?.GetType(), Right?.GetType());
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? "?" : "")}.{Right}";

        public override object Clone()
        {
            return new MemberAccess(Left.CloneCast(), Right.CloneCast(), null_conditional, SourceContext.CloneCast());
        }
    }
}
