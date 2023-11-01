using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class MemberAccess : ExpressionBinary
    {
        public class ClassAccess
        {
            public Type Name;
            public ClassAccess(Type name)
            {
                Name = name;
            }
            public override string ToString() => $"access to {Name.GetTypeString()}";
        }

        public override string Operator => ".";
        public MemberAccess(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public MemberAccess() : base() { }

        private FieldInfo field;
        private PropertyInfo prop;
        private Type nest_type;
        public object Create(ExecutionContext context)
        {
            var left = Left.ToString().Replace(" ", "") + $".{Right.Cast<InvokeLexem>().Name}";

            if (left != null)
            {
                if (Right is InvokeLexem invokeLexem)
                {
                    return Activator.CreateInstance(left.ToType(), invokeLexem.Arguments.Select(x => x.GetValue(context)).ToArray());
                }
            }

            throw new OperatorError(this, left?.GetType(), Right?.GetType());
        }

        private bool counted_contextwrapcache;
        private string variable_name;

        private bool counted_contextwrapcache2;
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);

            if (counted_contextwrapcache)
            {
                return (left as ExecutionContext.ContextWrap).pred.LocalVariables.GetValue(variable_name).Item1;
            }
            else if (counted_contextwrapcache2)
            {
                return (Right as InvokeLexem).GetValue((left as ExecutionContext.ContextWrap).pred, (Right as InvokeLexem).Arguments.Select(x => x.GetValue(context)).ToArray());
            }

            if (left != null)
            {
                if (left is ExecutionContext.ContextWrap pred)
                {
                    if (Right is NameLexem predName)
                    {
                        variable_name = predName.ToString().Replace(" ", "");
                        counted_contextwrapcache = true;
                        return pred.pred.LocalVariables.GetValue(variable_name).Item1;
                    }
                    else if (Right is InvokeLexem invokeLexem)
                    {
                        counted_contextwrapcache2 = true;
                        return invokeLexem.GetValue(pred.pred, invokeLexem.Arguments.Select(x => x.GetValue(context)).ToArray());
                    }
                }
                var has_access = left is ClassAccess access; 
                var type = has_access ? (left as ClassAccess).Name : left.GetType();
                if (field != null) return field.GetValue(left);
                if (prop != null) return prop.GetValue(left);
                if (nest_type != null) return new ClassAccess(nest_type);
                
                if (Right is NameLexem nameLexem)
                {
                    field = type.GetField(nameLexem.Name);
                    if (field != null) return field.GetValue(left);
                    prop = type.GetProperty(nameLexem.Name);
                    if (prop != null) return prop.GetValue(left);
                    nest_type = type.GetNestedType(nameLexem.Name);
                    if (nest_type != null) return new ClassAccess(nest_type);
                    throw new RuntimeError($"Name \"{nameLexem.Name}\" not found in {type.GetTypeString()}", SourceContext);
                }
                else if (Right is InvokeLexem invokeLexem)
                {
                    return invokeLexem.GetValue(context, left);
                }
            }

            
            throw new OperatorError(this, left?.GetType(), Right?.GetType());
        }

        private bool counted_other_context_assign;
        private string other_context_name;
        public void SetValue(ExecutionContext context, object value)
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
                    if (Right is NameLexem nameLexem2)
                    {
                        other_context_name = Right.ToString().Replace(" ", "");
                        counted_other_context_assign = true;
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
                if (Right is NameLexem nameLexem)
                {
                    field = type.GetField(nameLexem.Name);
                    if (field != null)
                    {
                        field.SetValue(left, value);
                        return;
                    }
                    prop = type.GetProperty(nameLexem.Name);
                    if (prop != null)
                    {
                        prop.SetValue(left, value);
                        return;
                    }
                    throw new RuntimeError($"Name \"{nameLexem.Name}\" not found in \"{type.Name.GetTypeString()}\"", SourceContext);
                }
            }

            throw new OperatorError(this, left?.GetType(), Right?.GetType());
        }
    }
}
