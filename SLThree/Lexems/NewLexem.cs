using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Linq;

namespace SLThree
{
    public class NewLexem : BaseLexem
    {
        public InvokeLexem InvokeLexem;
        public MemberAccess MemberAccess;

        public NewLexem(MemberAccess memberAccess, Cursor cursor) : base(cursor)
        {
            MemberAccess = memberAccess;
        }

        public NewLexem(InvokeLexem invokeLexem, Cursor cursor) : base(cursor)
        {
            InvokeLexem = invokeLexem;
        }

        public override string ToString() => $"new {MemberAccess}";

        public override object GetValue(ExecutionContext context)
        {
            if (MemberAccess != null) return MemberAccess.Create(context);
            else return Activator.CreateInstance(
                context.LocalVariables[InvokeLexem.Name.ToString()].Cast<MemberAccess.ClassAccess>().Name,
                InvokeLexem.Arguments.Select(x => x.GetValue(context)).ToArray());
        }
    }
}
