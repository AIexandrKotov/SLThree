using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class CreatorInstance : BaseExpression
    {
        /*
        ---Creating instances---
        new T;
        new T {};
        new T(args);
        new T(args) {};
        new T: TBase;
        new T: TBase {}; 
        new T(args): TBase;
        new T(args): TBase {};
        ---With name assignation
        new T Name;
        new T Name {};
        new T(args) Name;
        new T(args) Name {};
        new T Name: TBase;
        new T Name: TBase {}; 
        new T(args) Name: TBase;
        new T(args) Name: TBase {};
        */
        public static CreatorInstance CaseType(TypenameExpression type, SourceContext context)
            => new CreatorInstance(
                type, 
                null, 
                new BaseExpression[0], 
                null, 
                context);
        public static CreatorInstance CaseTypeBody(TypenameExpression type, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                new BaseExpression[0],
                new CreatorContext(null, new BaseExpression[0], body, false, context),
                context);
        public static CreatorInstance CaseTypeArgs(TypenameExpression type, BaseExpression[] args, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                args,
                null,
                context);
        public static CreatorInstance CaseTypeArgsBody(TypenameExpression type, BaseExpression[] args, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type, 
                null, 
                args,
                new CreatorContext(null, new BaseExpression[0], body, false, context),
                context);
        public static CreatorInstance CaseTypeInheritance(TypenameExpression type, BaseExpression[] ancestors, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                new BaseExpression[0],
                new CreatorContext(null, ancestors, null, false, context),
                context);
        public static CreatorInstance CaseTypeBodyInheritance(TypenameExpression type, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                new BaseExpression[0],
                new CreatorContext(null, ancestors, body, false, context),
                context);
        public static CreatorInstance CaseTypeArgsInheritance(TypenameExpression type, BaseExpression[] args, BaseExpression[] ancestors, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                args,
                null,
                context);
        public static CreatorInstance CaseTypeArgsBodyInheritance(TypenameExpression type, BaseExpression[] args, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                null,
                args,
                new CreatorContext(null, ancestors, body, false, context),
                context);
        public static CreatorInstance NamedCaseType(TypenameExpression type, BaseExpression name, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                new BaseExpression[0],
                null,
                context);
        public static CreatorInstance NamedCaseTypeBody(TypenameExpression type, BaseExpression name, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                new BaseExpression[0],
                new CreatorContext(null, new BaseExpression[0], body, false, context),
                context);
        public static CreatorInstance NamedCaseTypeArgs(TypenameExpression type, BaseExpression name, BaseExpression[] args, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                args,
                null,
                context);
        public static CreatorInstance NamedCaseTypeArgsBody(TypenameExpression type, BaseExpression name, BaseExpression[] args, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                args,
                new CreatorContext(name, new BaseExpression[0], body, false, context),
                context);
        public static CreatorInstance NamedCaseTypeInheritance(TypenameExpression type, BaseExpression name, BaseExpression[] ancestors, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                new BaseExpression[0],
                new CreatorContext(name, ancestors, null, false, context),
                context);
        public static CreatorInstance NamedCaseTypeBodyInheritance(TypenameExpression type, BaseExpression name, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                new BaseExpression[0],
                new CreatorContext(name, ancestors, body, false, context),
                context);
        public static CreatorInstance NamedCaseTypeArgsInheritance(TypenameExpression type, BaseExpression name, BaseExpression[] args, BaseExpression[] ancestors, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                args,
                null,
                context);
        public static CreatorInstance NamedCaseTypeArgsBodyInheritance(TypenameExpression type, BaseExpression name, BaseExpression[] args, BaseExpression[] ancestors, CreatorContextBody body, SourceContext context)
            => new CreatorInstance(
                type,
                name,
                args,
                new CreatorContext(name, ancestors, body, false, context),
                context);


        public CreatorInstance(TypenameExpression type, BaseExpression name, BaseExpression[] args, CreatorContext creatorContext, SourceContext context) : base(context)
        {
            Type = type;
            Name = name;
            Arguments = args;
            CreatorContext = creatorContext;
        }

        public TypenameExpression Type { get; set; }
        public BaseExpression Name { get; set; }
        public BaseExpression[] Arguments { get; set; }
        public CreatorContext CreatorContext { get; set; }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();
            sb.Append("new ");
            sb.Append(Type.ToString());
            if (Arguments.Length > 0)
                sb.Append($"({Arguments.JoinIntoString(", ")})");
            if (CreatorContext?.Name != null)
                sb.Append($" {CreatorContext.Name}");
            if (CreatorContext?.Ancestors.Length > 0)
            {
                sb.Append(": ");
                sb.Append(CreatorContext.Ancestors.JoinIntoString(", "));
            }
            if (CreatorContext?.CreatorBody != null)
            {
                sb.Append($"{{\n{CreatorContext.CreatorBody}\n}}");
            }
            return sb.ToString();
        }

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            object instance;
            var type = Type.GetValue(context).Cast<Type>();
            if (CreatorContext != null)
            {
                var created = CreatorContext.GetValue(context).Cast<ContextWrap>().Context;
                if (Arguments.Length == 0)
                    instance = type.InstanceUnwrap(created);
                else
                {
                    instance = Activator.CreateInstance(type, Arguments.ConvertAll(x => x.GetValue(context)));
                    type.InstanceUnwrapIn(created, instance);
                }
                if (CreatorContext.Name != null)
                    BinaryAssign.AssignToValue(context, CreatorContext.Name, instance, ref counted_invoked, ref is_name_expr, ref variable_index);
            }
            else
            {
                instance = Activator.CreateInstance(type, Arguments.ConvertAll(x => x.GetValue(context)));
                if (Name != null)
                    BinaryAssign.AssignToValue(context, Name, instance, ref counted_invoked, ref is_name_expr, ref variable_index);
            }
            return instance;
        }

        public override object Clone()
        {
            return new CreatorInstance(Type.CloneCast(), Name.CloneCast(), Arguments.CloneArray(), CreatorContext.CloneCast(), SourceContext.CloneCast());
        }
    }
}
