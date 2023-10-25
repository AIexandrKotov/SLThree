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

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);

            if (left != null)
            {
                if (left is ExecutionContext.PredWrap pred)
                {
                    if (Right is NameLexem predName)
                    {
                        return pred.pred.LocalVariables[predName.ToString().Replace(" ", "")];
                    }
                    else if (Right is InvokeLexem invokeLexem)
                    {
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
                }
                else if (Right is InvokeLexem invokeLexem)
                {
                    return invokeLexem.GetValue(context, left);
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), Right?.GetType());
        }
        public void SetValue(ExecutionContext context, object value)
        {
            var left = Left.GetValue(context);

            if (left != null)
            {
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
