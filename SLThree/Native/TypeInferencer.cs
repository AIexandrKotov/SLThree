using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Native
{
    public class TypeInferencer
    {
        public class ReturnCollector : AbstractVisitor
        {
            public List<ReturnStatement> Returns = new List<ReturnStatement>();

            public override void VisitStatement(ReturnStatement statement)
            {
                Returns.Add(statement);
                base.VisitStatement(statement);
            }

            public static List<ReturnStatement> Collect(IList<BaseStatement> statements)
            {
                var visitor = new ReturnCollector();
                foreach (var x in statements)
                    visitor.VisitStatement(x);
                return visitor.Returns;
            }
        }

        public static Type ReconstructReturnType(Method method)
        {
            var statements = method.Statements.Statements;
            var returns = ReturnCollector.Collect(statements);
            if (returns.Count == 0 || returns.All(x => x.Expression == null)) return typeof(void);
            throw new NotSupportedException("Non-void returns not supported right now");
        }
    }
}
