using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class NewLexem : BaseLexem
    {
        public InvokeLexem InvokeLexem;
        public MemberAccess MemberAccess;

        public NewLexem() : base() { }

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

        public override object Clone() => new NewLexem()
        {
            MemberAccess = MemberAccess.CloneCast(),
            InvokeLexem = InvokeLexem.CloneCast(),
            SourceContext = SourceContext.CloneCast(),
        };
    }
}
