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

        private bool counted_name;
        private string invk_name;
        public override object GetValue(ExecutionContext context)
        {
            if (MemberAccess != null) return MemberAccess.Create(context);
            else
            {
                if (!counted_name)
                {
                    invk_name = InvokeLexem.Name.ToString();
                    counted_name = true;
                }
                return Activator.CreateInstance(
                context.LocalVariables.GetValue(invk_name).Item1.Cast<MemberAccess.ClassAccess>().Name,
                InvokeLexem.Arguments.Select(x => x.GetValue(context)).ToArray());
            }
        }
    }
}
