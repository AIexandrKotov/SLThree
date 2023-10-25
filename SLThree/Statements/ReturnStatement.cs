using Pegasus.Common;

namespace SLThree
{
    public class ReturnStatement : BaseStatement
    {
        public bool VoidReturn;
        public BaseLexem Lexem;

        public ReturnStatement(BaseLexem lexem, Cursor cursor) : base(cursor)
        {
            Lexem = lexem;
        }

        public ReturnStatement(Cursor cursor) : base(cursor)
        {
            VoidReturn = true;
        }

        public override string ToString() => $"{Lexem}";
        public override object GetValue(ExecutionContext context)
        {
            if (VoidReturn)
            {
                context.Return();
                return null;
            }
            else
            {
                var value = Lexem.GetValue(context);
                context.Return(value);
                return value;
            }
        }
    }
}
