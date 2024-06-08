using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;

namespace SLThree
{
    public class CreatorContextBody : StatementList, ICloneable
    {
        public CreatorContextBody() : base() { }
        public CreatorContextBody(IList<BaseStatement> statements, SourceContext context) : base(statements, context) { }

        public ExecutionContext GetValue(ExecutionContext target, ExecutionContext call)
        {
            for (var i = 0; i < count; i++)
            {
                if (Statements[i] is ExpressionStatement es && es.Expression is BinaryAssign assign)
                    assign.AssignValue(target, assign.Left, assign.Right.GetValue(call));
            }
            return target;
        }

        public override object GetValue(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new CreatorContextBody(Statements.CloneArray(), SourceContext.CloneCast());
        }
    }
}
