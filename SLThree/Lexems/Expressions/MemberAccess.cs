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
        }

        public override string Operator => ".";
        public MemberAccess(BoxSupportedLexem left, BoxSupportedLexem right, Cursor cursor) : base(left, right, cursor) { }
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

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }

        private bool counted_contextwrapcache;
        private string variable_name;

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).Boxed();

            if (counted_contextwrapcache)
            {
                reference = (left as ExecutionContext.ContextWrap).pred.LocalVariables.GetValue(variable_name).Item1.ToSpeedy();
                return ref reference;
            }

            if (left != null)
            {
                if (left is ExecutionContext.ContextWrap pred)
                {
                    if (Right is NameLexem predName)
                    {
                        variable_name = predName.ToString().Replace(" ", "");
                        counted_contextwrapcache = true;
                        return ref pred.pred.LocalVariables.GetValue(variable_name).Item1.ToSpeedy(ref reference);
                    }
                    else if (Right is InvokeLexem invokeLexem)
                    {
                        return ref invokeLexem.GetValue(pred.pred, invokeLexem.Arguments.Select(x => x.GetValue(context)).ToArray()).ToSpeedy(ref reference);
                    }
                }
                var has_access = left is ClassAccess access;
                var type = has_access ? (left as ClassAccess).Name : left.GetType();
                if (field != null)
                {
                    return ref field.GetValue(left).ToSpeedy(ref reference);
                }
                if (prop != null) return ref prop.GetValue(left).ToSpeedy(ref reference);
                if (nest_type != null) return ref new ClassAccess(nest_type).ToSpeedy(ref reference);

                if (Right is NameLexem nameLexem)
                {
                    field = type.GetField(nameLexem.Name);
                    if (field != null) return ref field.GetValue(left).ToSpeedy(ref reference);
                    prop = type.GetProperty(nameLexem.Name);
                    if (prop != null) return ref prop.GetValue(left).ToSpeedy(ref reference);
                    nest_type = type.GetNestedType(nameLexem.Name);
                    if (nest_type != null) return ref new ClassAccess(nest_type).ToSpeedy(ref reference);
                }
                else if (Right is InvokeLexem invokeLexem)
                {
                    return ref invokeLexem.GetValue(context, left).ToSpeedy(ref reference);
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }

        private bool counted_other_context_assign;
        private string other_context_name;
        public void SetValue(ExecutionContext context, SLTSpeedyObject value)
        {
            var left = Left.GetValue(context).Boxed();

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
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }
        private void SetValue(ExecutionContext context, object value)
        {
            var left = Left.GetValue(context).Boxed();

            if (counted_other_context_assign)
            {
                (left as ExecutionContext.ContextWrap).pred.LocalVariables.SetValue(other_context_name, value.ToSpeedy());
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
                        context.LocalVariables.SetValue(other_context_name, value.ToSpeedy());
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
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }
    }
}
