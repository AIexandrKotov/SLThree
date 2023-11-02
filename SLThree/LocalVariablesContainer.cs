using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class LocalVariablesContainer
    {
        public LocalVariablesContainer()
        {

        }

        internal int current = 0;
        internal object[] Variables = new object[8];
        public Dictionary<string, int> NamedIdenificators = new Dictionary<string, int>();

        public Dictionary<string, object> GetAsDictionary() => NamedIdenificators.ToDictionary(x => x.Key, x => Variables[x.Value]);

        public void ClearNulls()
        {
            NamedIdenificators = NamedIdenificators.Where(x => Variables[x.Value] != null).ToDictionary(x => x.Key, x => x.Value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Expand()
        {
            var nw = new object[Variables.Length * 2];
            Variables.CopyTo(nw, 0);
            Variables = nw;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArguments(Method method, object[] args)
        {
            if (Variables.Length <= args.Length) Variables = new object[8 + args.Length + Variables.Length];
            if (current <= args.Length) current = args.Length;
            args.CopyTo(Variables, 0);
            for (var i = 0; i < args.Length; i++)
                NamedIdenificators[method.ParamNames[i]] = i;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetValue(string name, object value)
        {
            if (NamedIdenificators.TryGetValue(name, out var id))
            {
                Variables[id] = value;
                return id;
            }
            else
            {
                if (current >= Variables.Length) Expand();
                Variables[current] = value;
                NamedIdenificators[name] = current;
                return current++;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int index, object value)
        {
            Variables[index] = value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}
