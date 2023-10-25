using Pegasus.Common;
using SLThree.Extensions;
using System.Linq;

namespace SLThree
{
    public class UsingStatement : BaseStatement
    {
        public BaseLexem Lexem;
        public string Name;

        public UsingStatement(BaseLexem lexem, string name, Cursor cursor) : base(cursor)
        {
            Lexem = lexem;
            Name = name;
        }
        public UsingStatement(BaseLexem lexem, Cursor cursor) : base(cursor)
        {
            Lexem = lexem;
            Name = Lexem.ToString().Split('.').Last().Replace(" ", "");
        }

        public override string ToString() => $"using {Lexem} as {Name}";
        public override object GetValue(ExecutionContext context)
        {
            context.LocalVariables[Name] = new MemberAccess.ClassAccess(Lexem.ToString().Replace(" ", "").ToType());
            return null;
        }
    }
}
