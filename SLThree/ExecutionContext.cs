using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class ExecutionContext
    {
        public interface IExecutable
        {
            object GetValue(ExecutionContext context);
        }

        public void Return() { }
        public ExecutionContext PrepareToInvoke() { return this; }

        public Dictionary<string, object> LocalVariables { get; set; } = new Dictionary<string, object>();
    }
}
