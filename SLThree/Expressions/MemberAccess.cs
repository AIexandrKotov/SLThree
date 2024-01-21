using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SLThree
{
    public class MemberAccess : BinaryOperator
    {
        public class ClassAccess
        {
            public Type Name;
            public ClassAccess(Type name)
            {
                Name = name;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{Name.GetTypeString()} {{");
                //var methods = Name.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var static_methods = Name.GetMethods(BindingFlags.Public | BindingFlags.Static);
                //var fields = Name.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var static_fields = Name.GetFields(BindingFlags.Public | BindingFlags.Static);
                //var props = Name.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var static_props = Name.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var x in static_fields)
                {
                    sb.AppendLine($"    {x.FieldType.GetTypeString()} {x.Name};");
                }
                /*if (!Name.IsSealed && !Name.IsAbstract)
                foreach (var x in fields)
                {
                    sb.AppendLine($"    {x.Name};");
                }*/
                foreach (var x in static_props)
                {
                    sb.Append($"    {x.PropertyType.GetTypeString()} {x.Name} {{ ");
                    if (x.GetMethod != null) sb.Append("get; ");
                    if (x.SetMethod != null) sb.Append("set; ");
                    sb.AppendLine("}");
                }
                /*if (!Name.IsSealed && !Name.IsAbstract)
                    foreach (var x in props)
                {
                    sb.Append($"    {x.Name} {{ ");
                    if (x.GetMethod != null) sb.Append("get; ");
                    if (x.SetMethod != null) sb.Append("set; ");
                    sb.AppendLine("}");
                }*/
                foreach (var x in static_methods)
                {
                    if (x.Name.StartsWith("get_") || x.Name.StartsWith("set_")) continue;
                    sb.Append($"    {x.ReturnType.GetTypeString()} {x.Name}(");
                    sb.Append(x.GetParameters().ConvertAll(p => p.ParameterType.GetTypeString()).JoinIntoString(", "));
                    sb.AppendLine(");");
                }
                /*if (!Name.IsSealed && !Name.IsAbstract)
                    foreach (var x in methods)
                {
                    if (x.Name.StartsWith("get_") || x.Name.StartsWith("set_")) continue;
                    sb.Append($"    {x.Name}(");
                    sb.Append(x.GetParameters().ConvertAll(p => p.ParameterType.GetTypeString()).JoinIntoString(", "));
                    sb.AppendLine(");");
                }*/

                sb.AppendLine("}");

                return sb.ToString();
            }
        }

        public override string Operator => ".";
        public MemberAccess(BaseExpression left, BaseExpression right, SourceContext context) : base(left, right, context) { }
        public MemberAccess(BaseExpression left, BaseExpression right, bool null_conditional, SourceContext context) : base(left, right, context) {
            this.null_conditional = null_conditional;
        }
        public MemberAccess() : base() { }

        private FieldInfo field;
        private PropertyInfo prop;
        private Type nest_type;
        private bool null_conditional;

        private bool counted_contextwrapcache;
        private string variable_name;

        private bool counted_contextwrapcache2;
        private bool is_unwrap;
        private bool is_super;
        private bool is_upper;
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);

            if (counted_contextwrapcache)
            {
                if (is_super) return (left as ExecutionContext.ContextWrap).pred.super;
                else if (is_upper) return (left as ExecutionContext.ContextWrap).pred.PreviousContext.wrap;
                else return (left as ExecutionContext.ContextWrap).pred.LocalVariables.GetValue(variable_name).Item1;
            }
            else if (counted_contextwrapcache2)
            {
                if (is_unwrap) return (left as ExecutionContext.ContextWrap).pred;
                else return (Right as InvokeExpression).GetValue((left as ExecutionContext.ContextWrap).pred, (Right as InvokeExpression).Arguments.ConvertAll(x => x.GetValue(context)));
            }

            if (left != null)
            {
                if (left is ExecutionContext.ContextWrap pred)
                {
                    if (Right is NameExpression predName)
                    {
                        if (predName.Name == "super")
                        {
                            counted_contextwrapcache = true;
                            is_super = true;
                            return pred.pred.super;
                        }
                        else if (predName.Name == "upper")
                        {
                            counted_contextwrapcache = true;
                            is_upper = true;
                            return pred.pred.PreviousContext.wrap;
                        }
                        variable_name = predName.ExpressionToString().Replace(" ", "");
                        counted_contextwrapcache = true;
                        return pred.pred.LocalVariables.GetValue(variable_name).Item1;
                    }
                    else if (Right is InvokeExpression invokeExpression)
                    {
                        counted_contextwrapcache2 = true;
                        if (invokeExpression.Left?.TryCastRef<NameExpression>()?.Name == "unwrap" && invokeExpression.Arguments.Length == 0)
                        {
                            is_unwrap = true;
                            return pred.pred;
                        }
                        return invokeExpression.GetValue(pred.pred, invokeExpression.Arguments.Select(x => x.GetValue(context)).ToArray());
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
                    throw new RuntimeError($"Name \"{nameExpression.Name}\" not found in {type.GetTypeString()}", SourceContext);
                }
                else if (Right is InvokeExpression invokeExpression)
                {
                    return invokeExpression.GetValue(context, left);
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
                (left as ExecutionContext.ContextWrap).pred.LocalVariables.SetValue(other_context_name, value);
                return;
            }

            if (left != null)
            {
                if (left is ExecutionContext.ContextWrap wrap)
                {
                    context = wrap.pred;
                    var has_access_2 = left is ClassAccess access_2;
                    var type_2 = has_access_2 ? (left as ClassAccess).Name : left.GetType();
                    if (Right is NameExpression nameExpression2)
                    {
                        other_context_name = Right.ExpressionToString().Replace(" ", "");
                        counted_other_context_assign = true;
                        if (value is Method mth)
                        {
                            mth = mth.CloneCast();
                            mth.Name = nameExpression2.Name;
                            mth.UpdateContextName();
                            mth.DefinitionPlace = new ExecutionContext.ContextWrap(context);
                            value = mth;
                        }
                        context.LocalVariables.SetValue(other_context_name, value);
                    }
                    return;
                }
                var has_access = left is ClassAccess access;
                var type = has_access ? (left as ClassAccess).Name : left.GetType();
                if (field != null)
                {
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
                        field.SetValue(left, value);
                        return;
                    }
                    prop = type.GetProperty(nameExpression.Name);
                    if (prop != null)
                    {
                        prop.SetValue(left, value);
                        return;
                    }
                    throw new RuntimeError($"Name \"{nameExpression.Name}\" not found in \"{type.GetTypeString()}\"", SourceContext);
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
