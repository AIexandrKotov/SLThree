using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class LocalVariablesContainer
    {
        public LocalVariablesContainer(int default_size = 8, Dictionary<string, int> names = null)
        {
            Variables = new object[default_size];
            NamedIdentificators = names ?? new Dictionary<string, int>();
            current = NamedIdentificators.Count;
        }

        internal int current = 0;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public object[] Variables;
        public Dictionary<string, int> NamedIdentificators;

        public Dictionary<string, object> GetAsDictionary() => NamedIdentificators.ToDictionary(x => x.Key, x => Variables[x.Value]);
        public static LocalVariablesContainer GetFromDictionary(IDictionary<string, object> objs)
        {
            var ret = new LocalVariablesContainer(objs.Count, objs.Keys.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i));
            objs.Values.ToArray().CopyTo(ret.Variables, 0);
            return ret;
        }

        public void ClearNulls()
        {
            NamedIdentificators = NamedIdentificators.Where(x => Variables[x.Value] != null).ToDictionary(x => x.Key, x => x.Value);
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
                NamedIdentificators[method.ParamNames[i]] = i;
        }
        public void FillOther(LocalVariablesContainer container)
        {
            foreach (var x in container.NamedIdentificators)
                SetValue(x.Key, container.Variables[x.Value]);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetValue(string name, object value)
        {
            if (NamedIdentificators.TryGetValue(name, out var id))
            {
                Variables[id] = value;
                return id;
            }
            else
            {
                if (current >= Variables.Length) Expand();
                Variables[current] = value;
                NamedIdentificators[name] = current;
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
            if (NamedIdentificators.TryGetValue(name, out var x))
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
