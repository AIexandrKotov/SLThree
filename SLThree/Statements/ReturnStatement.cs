using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Net.NetworkInformation;

namespace SLThree
{
    public class ReturnStatement : BaseStatement
    {
        public bool VoidReturn;
        public BaseLexem Lexem;

        public ReturnStatement() : base() { }
        public ReturnStatement(BaseLexem lexem, SourceContext context) : base(context)
        {
            Lexem = lexem;
        }

        public ReturnStatement(SourceContext context) : base(context)
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

        public override object Clone()
        {
            return new ReturnStatement() { Lexem = Lexem.CloneCast(), VoidReturn = VoidReturn.Copy(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
