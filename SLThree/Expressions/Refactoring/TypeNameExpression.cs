using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Expressions.Refactoring
{
    public class TypenameExpression : BaseExpression
    {
        // Refactoring

        // @X - UnaryRuntimeReflection (compile-time types has low priority)
        // @@X - UnaryReflection (only compile-time types)
        // x::y - BinaryReflection

        // x<y*> - TypeExpression
        // x as <TypeExpression>
        // x is <TypeExpression>
        // using <TypeExpression>
        // context: <TypeExpression>
        // new context: <TypeExpression> { }
        // new using <TypeExpression>
        // new <TypeExpression>(values)
        // 

        public BaseExpression Typename;
        public TypenameExpression[] Generics;

        private string type_str;
        private bool is_array;

        private bool type_cached;
        private int type_variable_id;

        private Type static_type;

        public TypenameExpression() : base() { }

        public TypenameExpression(BaseExpression expression, TypenameExpression[] generics, SourceContext context) : base(context)
        {
            Typename = expression;
            Generics = generics;
            type_str = Typename.ToString() + (Generics == null ? "" : $"`{Generics.Length}");
            if (type_str == "array`1" || type_str == "array`2") is_array = true;
        }

        public TypenameExpression(BaseExpression expression, SourceContext context) : this(expression, null, context)
        {
            
        }

        public override string ExpressionToString()
        {
            return $"{type_str}" + (Generics != null ? $"<{Generics.JoinIntoString(", ")}>" : "");
        }

        public Type GetStaticValue()
        {
            if (static_type != null)
                return static_type;
            static_type = type_str.ToType();
            if (static_type == null)
                throw new LogicalError($"Type \"{type_str}\" not found", SourceContext);
            if (!is_array)
            {
                if (Generics == null) return static_type;
                return static_type = static_type.MakeGenericType(Generics.ConvertAll(x => x.GetStaticValue()));
            }
            var indexes = Generics.Length > 1 ? Generics[1].Typename.Cast<LongLiteral>().Value.Cast<long>() : 1;
            return static_type = Generics[0].GetStaticValue().MakeArrayType((int)indexes);
        }

        public object GetStaticValue(ExecutionContext context)
        {
            if (static_type != null)
                return static_type;
            static_type = type_str.ToType();
            if (static_type == null)
                throw new RuntimeError($"Type \"{type_str}\" not found", SourceContext);
            if (!is_array)
            {
                if (Generics == null) return static_type;
                return static_type.MakeGenericType(Generics.ConvertAll(x => Isolate(x.GetValue(context))));
            }
            var indexes = Generics.Length > 1 ? Generics[1].Typename.GetValue(context).Cast<long>() : 1;
            return Isolate(Generics[0].GetValue(context)).MakeArrayType((int)indexes);
        }

        public Type Isolate(object o)
        {
            if (o is Type type) return type;
            if (o is MemberAccess.ClassAccess access) return access.Name;
            throw new RuntimeError($"{o?.GetType().GetTypeString() ?? "null"} unconvertible to Type", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            if (type_cached)
            {
                return context.LocalVariables.GetValue(type_variable_id);
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
                return Isolate(Generics[0].GetValue(context)).MakeArrayType(1);
            }
            return GetStaticValue(context);
        }

        public override object Clone()
        {
            return new TypenameExpression(Typename.CloneCast(), Generics?.CloneArray(), SourceContext.CloneCast());
        }
    }

    public class CastExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression Type;

        public CastExpression(BaseExpression left, TypenameExpression type, SourceContext context) : base(context)
        {
            Left = left;
            Type = type;
        }

        public override string ExpressionToString() => $"{Left} as {Type}";

        public override object GetValue(ExecutionContext context)
        {
            return WrappersTypeSetting.UnwrapCast(Type.GetValue(context).Cast<Type>(), Left.GetValue(context));
        }

        public override object Clone()
        {
            return new CastExpression(Left.CloneCast(), Type.CloneCast(), SourceContext.CloneCast());
        }
    }

    public class ExpressionBinaryIs : ExpressionBinary
    {
        public ExpressionBinaryIs(BaseExpression left, BaseExpression right, SourceContext context) : base(left, right, context)
        {

        }

        public override string Operator => "is";

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context).GetType().IsType(Right.GetValue(context).Cast<Type>());
        }

        public override object Clone()
        {
            return new ExpressionBinaryIs(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }

    public class CreatorUsing : BaseExpression
    {
        public TypenameExpression Type;

        public CreatorUsing(TypenameExpression type, SourceContext context) : base(context)
        {
            Type = type;
        }

        public override string ExpressionToString() => $"new using {Type}";

        public override object GetValue(ExecutionContext context)
        {
            return new MemberAccess.ClassAccess(Type.GetValue(context).Cast<Type>());
        }

        public override object Clone()
        {
            return new CreatorUsing(Type.CloneCast(), SourceContext.CloneCast());
        }
    }

    public class CreatorContext : BaseExpression
    {
        // new context
        // new context { ... }
        // new context A
        // new context A { ... }
        // new context : T
        // new context : T { ... }
        // new context A : T
        // new context A : T { ... }

        public NameExpression Name;
        public TypenameExpression Typecast;
        public BaseStatement[] Body;

        public bool HasCast => Typecast != null;
        public bool HasName => Name != null;
        public bool HasBody => Body.Length > 0;

        public CreatorContext(SourceContext context) : this(null, null, new BaseStatement[0], context) { }

        public CreatorContext(NameExpression name, SourceContext context) : this(name, null, new BaseStatement[0], context) { }

        public CreatorContext(BaseStatement[] body, SourceContext context) : this(null, null, body, context) { }

        public CreatorContext(NameExpression name, TypenameExpression typecast, BaseStatement[] body, SourceContext context) : base(context)
        {
            Name = name;
            Typecast = typecast;
            Body = body;
        }

        public override string ExpressionToString() => $"new context {(HasName ? Name.Name : "")} {(HasCast ? $": {Typecast}" : "")} {{\n{Body.JoinIntoString("\n")}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = new ExecutionContext(context);
            if (HasName) ret.Name = Name.Name;
            if (HasBody)
            {
                for (var i = 0; i < Body.Length; i++)
                {
                    if (Body[i] is ExpressionStatement es && es.Expression is ExpressionBinaryAssign assign)
                        assign.AssignValue(ret, assign.Left, assign.Right.GetValue(context));
                    else if (Body[i] is ContextStatement cs)
                        cs.GetValue(ret);
                }
            }
            if (HasCast) return ret.CastToType(Typecast.GetValue(context).Cast<Type>());
            return new ExecutionContext.ContextWrap(ret);
        }

        public override object Clone() => new CreatorContext(Name.CloneCast(), Typecast.CloneCast(), Body?.CloneArray(), SourceContext.CloneCast());
    }

    public class UsingStatement : BaseStatement
    {
        public NameExpression Alias;
        public CreatorUsing Using;

        public UsingStatement(NameExpression alias, CreatorUsing usingBody, SourceContext context) : base(context)
        {
            Alias = alias;
            Using = usingBody;
        }

        public UsingStatement(CreatorUsing @using, SourceContext context) : this(null, @using, context) { }

        public override string ToString() => $"using {Using.Type}";

        public override object GetValue(ExecutionContext context)
        {
            var @using = Using.GetValue(context).Cast<Type>();
            var name = Alias == null ? @using.Name : Alias.Name;
            context.LocalVariables.SetValue(name, @using);
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement(Using.CloneCast(), SourceContext.CloneCast());
        }
    }

    public class ContextStatement : BaseStatement
    {
        public CreatorContext Creator;

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public override object GetValue(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
