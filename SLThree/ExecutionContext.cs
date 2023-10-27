using System;
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

        public bool ForbidImplicit = true;

        public bool Returned;
        public bool Broken;
        public bool Continued;

        public object ReturnedValue;

        public static ContextWrap global = new ContextWrap(new ExecutionContext());

        private static bool and(bool a, bool b) => a && b;
        private static bool or(bool a, bool b) => a || b;
        private static bool xor(bool a, bool b) => a ^ b;
        static ExecutionContext()
        {
            global.pred.LocalVariables.SetValue("and", Method.Create<bool, bool, bool>(and));
            global.pred.LocalVariables.SetValue("or", Method.Create<bool, bool, bool>(or));
            global.pred.LocalVariables.SetValue("xor", Method.Create<bool, bool, bool>(xor));
        }

        public class ContextWrap
        {
            public ExecutionContext pred;
            public ContextWrap(ExecutionContext pred)
            {
                this.pred = pred;
            }
        }
        internal ExecutionContext PreviousContext;
        public ContextWrap pred => new ContextWrap(PreviousContext);
        public ContextWrap direct => new ContextWrap(this);

        private int cycles = 0;

        public void StartCycle()
        {
            cycles += 1;
            Broken = Continued = false;
        }

        public bool InCycle() => cycles > 1;

        public void EndCycle()
        {
            cycles -= 1;
            Broken = Continued = false;
        }

        public void Return() { Returned = true; ReturnedValue = null; }
        public void Return(object o) { Returned = true; ReturnedValue = o; }
        public void Break() { Broken = true; }
        public void Continue() { Continued = true; }
        public void PrepareToInvoke() { Returned = Broken = Continued = false; }
        public void DefaultEnvironment()
        {
            
        }

        public BaseStatement parse(string s) => new Parser().ParseScript(s);
        public object eval(IExecutable executable) => executable.GetValue(this);

        public LocalVariablesContainer LocalVariables { get; set; } = new LocalVariablesContainer();
    }
}
