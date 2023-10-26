using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class LocalVariablesContainer
    {
        internal object[] Variables = new object[8];
        public Dictionary<string, int> NamedIdenificators = new Dictionary<string, int>();

        public Dictionary<string, object> GetAsDictionary() => NamedIdenificators.ToDictionary(x => x.Key, x => Variables[x.Value]);

        public void ClearNulls()
        {
            NamedIdenificators = NamedIdenificators.Where(x => Variables[x.Value] != null).ToDictionary(x => x.Key, x => x.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Expand()
        {
            var nw = new object[Variables.Length * 2];
            Variables.CopyTo(nw, 0);
            Variables = nw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetValue(string name, object value)
        {
            if (NamedIdenificators.TryGetValue(name, out var id))
            {
                Variables[id] = value;
                return id;
            }
            else
            {
                if (NamedIdenificators.Count >= Variables.Length) Expand();
                var count = NamedIdenificators.Count;
                Variables[count] = value;
                NamedIdenificators[name] = count;
                return count;
            }
        }
        public void SetValue(int index, object value)
        {
            Variables[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (object, int) GetValue(string name)
        {
            if (NamedIdenificators.TryGetValue(name, out var x))
            {
                return (Variables[x], x);
            }
            return (null, -1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(int index)
        {
            return Variables[index];
        }

        public LocalVariablesContainer()
        {
            
        }
    }

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

        public static ContextWrap global = new ContextWrap(new ExecutionContext());

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

        public LocalVariablesContainer LocalVariables { get; set; } = new LocalVariablesContainer();
    }
}
