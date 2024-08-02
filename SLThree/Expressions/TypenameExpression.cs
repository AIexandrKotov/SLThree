using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class TypenameExpression : BaseExpression
    {
        public BaseExpression Typename;
        public TypenameExpression[] Generics;

        private string type_str;
        internal bool is_array;
        public int array_inds = 1;

        private bool type_cached;
        private int type_variable_id;

        private Type static_type;

        public TypenameExpression() : base() { }

        public TypenameExpression(BaseExpression expression, TypenameExpression[] generics, SourceContext context) : base(context)
        {
            Typename = expression;
            Generics = generics;
            PullTheType();
        }

        public TypenameExpression(BaseExpression expression, SourceContext context) : this(expression, null, context)
        {

        }

        public void PullTheType()
        {
            is_array = false;
            type_str = Typename.ToString() + (Generics == null ? "" : $"`{Generics.Length}");
            if (type_str.StartsWith("array"))
            {
                var skipped = type_str.Skip(5).JoinIntoString("").Split('`');
                if (Generics?.Length == 1)
                {
                    is_array = true;
                    if (int.TryParse(skipped[0], out var rank)) array_inds = rank;
                }
            }
        }

        public override string ExpressionToString()
        {
            var gind = type_str.IndexOf('`');
            return $"{(gind != -1 ? type_str.Remove(gind) : type_str)}" + (Generics != null ? $"<{Generics.JoinIntoString(", ")}>" : "");
        }

        public virtual Type GetStaticValue()
        {
            if (static_type != null)
                return static_type;
            static_type = type_str.ToType();
            if (!is_array)
            {
                if (static_type == null)
                    throw new LogicalError($"Type \"{type_str}\" not found", SourceContext);
                if (Generics == null) return static_type;
                return static_type = static_type.MakeGenericType(Generics.ConvertAll(x => x.GetStaticValue()));
            }
            return static_type = array_inds == 1 ? Generics[0].GetStaticValue().MakeArrayType() : Generics[0].GetStaticValue().MakeArrayType(array_inds);
        }

        public object GetStaticValue(ExecutionContext context)
        {
            if (!is_array)
            {
                static_type = type_str.ToType();
                if (static_type == null)
                    throw new RuntimeError($"Type \"{type_str}\" not found", SourceContext);
                if (Generics == null) return static_type;
                return static_type.MakeGenericType(Generics.ConvertAll(x => Isolate(x.GetValue(context))));
            }
            return array_inds == 1 ? Isolate(Generics[0].GetValue(context)).MakeArrayType() : Isolate(Generics[0].GetValue(context)).MakeArrayType((int)array_inds);
        }

        public Type Isolate(object o)
        {
            if (o is Type type) return type;
            if (o is ClassAccess access) return access.Name;
            throw new RuntimeError($"{o?.GetType().GetTypeString() ?? "null"} unconvertible to Type", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            if (type_cached)
            {
                return Isolate(context.LocalVariables.GetValue(type_variable_id));
            }
            var type = context.LocalVariables.GetValue(type_str);
            if (type.Item1 != null)
            {
                type_cached = true;
                type_variable_id = type.Item2;
                if (!is_array)
                {
                    if (Generics == null) return Isolate(type.Item1);
                    return Isolate(type.Item1).MakeGenericType(Generics.ConvertAll(x => Isolate(x.GetValue(context))));
                }
                return array_inds == 1 ? Isolate(Generics[0].GetValue(context)).MakeArrayType(array_inds) : Isolate(Generics[0].GetValue(context)).MakeArrayType(array_inds);
            }
            return GetStaticValue(context);
        }

        public override object Clone()
        {
            return new TypenameExpression(Typename.CloneCast(), Generics?.CloneArray(), SourceContext.CloneCast());
        }
    }

    internal class TypenameGenericPart : TypenameExpression
    {
        public Type Type { get; set; }
        public override string ExpressionToString() => Type.GetTypeString();
        public override Type GetStaticValue()
        {
            return Type;
        }
        public override object GetValue(ExecutionContext context)
        {
            return Type;
        }
        public override object Clone()
        {
            return new TypenameGenericPart() { Type = Type, };
        }
    }
}
