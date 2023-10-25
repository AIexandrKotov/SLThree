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

        public bool Returned;
        public bool Broken;
        public bool Continued;

        public object ReturnedValue;

        public class PredWrap
        {
            public ExecutionContext pred;
            public PredWrap(ExecutionContext pred)
            {
                this.pred = pred;
            }
        }
        internal ExecutionContext PreviousContext;
        public PredWrap pred => new PredWrap(PreviousContext);

        public void Return() { Returned = true; ReturnedValue = null; }
        public void Return(object o) { Returned = true; ReturnedValue = o; }
        public void Break() { Broken = true; }
        public void Continue() { Continued = true; }
        public ExecutionContext PrepareToInvoke() { Returned = Broken = Continued = false; return this; }
        public void DefaultEnvironment()
        {
            
        }

        public BaseStatement parse(string s) => new Parser().ParseScript(s);
        public object eval(IExecutable executable) => executable.GetValue(this);

        public Dictionary<string, object> LocalVariables { get; set; } = new Dictionary<string, object>();
    }
}
