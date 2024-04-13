using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
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

        public override string ToString() => $"using {Using.Type} as {Alias}";

        public override object GetValue(ExecutionContext context)
        {
            var @using = Using.GetValue(context).Cast<ClassAccess>();
            string name;
            if (Alias == null)
            {
                var type_name = Using.GetTypenameWithoutGenerics();
                name = type_name.Contains(".") ? @using.Name.Name : type_name;
            }
            else name = Alias.Name;
            context.LocalVariables.SetValue(name, @using);
            return null;
        }

        public override object Clone()
        {
            return new UsingStatement(Using.CloneCast(), SourceContext.CloneCast());
        }
    }
}
