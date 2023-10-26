using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class ConditionStatement : BaseStatement
    {
        public BaseLexem Condition { get; set; }
        public BaseStatement TrueBlock { get; set; }
        public BaseStatement FalseBlock { get; set; }

        public ConditionStatement(BaseLexem condition, BaseStatement trueBlock, BaseStatement falseBlock, Cursor cursor) : base(cursor)
        {
            Condition = condition;
            TrueBlock = trueBlock;
            FalseBlock = falseBlock;
        }

        public override string ToString() => $"if ({Condition}) {{{TrueBlock}}}{(FalseBlock == null ? $"{{{FalseBlock}}}" : "")}";

        public override object GetValue(ExecutionContext context)
            => Condition.GetValue(context).AsBool
            ? TrueBlock.GetValue(context) 
            : FalseBlock?.GetValue(context);
    }
}
