using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Pascal
{
    public class Restorator : DefaultRestorator, IRestorator
    {
        public string Restore(BaseStatement statement, ExecutionContext context) => Restore<Restorator>(statement, context);
        public string Restore(BaseExpression expression, ExecutionContext context) => Restore<Restorator>(expression, context);
    }
}
