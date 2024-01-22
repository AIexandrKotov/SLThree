using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Visitors
{
    public interface IVisitor : IExpressionVisitor, IStatementVisitor
    {
        void VisitAny(object o);
        void Visit(Method method);
        void Visit(RecursiveMethod method);
        void Visit(ExecutionContext context);
    }
}
