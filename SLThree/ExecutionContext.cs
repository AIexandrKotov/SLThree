using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class LocalVariablesContainer
    {
        internal SLTSpeedyObject[] Variables = new SLTSpeedyObject[8];
        public Dictionary<string, int> NamedIdenificators = new Dictionary<string, int>();

        public Dictionary<string, object> GetAsDictionary() => NamedIdenificators.ToDictionary(x => x.Key, x => Variables[x.Value].Boxed());

        public void ClearNulls()
        {
            NamedIdenificators = NamedIdenificators.Where(x => Variables[x.Value].Boxed() != null).ToDictionary(x => x.Key, x => x.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Expand()
        {
            var nw = new SLTSpeedyObject[Variables.Length * 2];
            Variables.CopyTo(nw, 0);
            Variables = nw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArguments(Method method, SLTSpeedyObject[] args)
        {
            if (Variables.Length <= args.Length) Variables = new SLTSpeedyObject[8 + args.Length + Variables.Length];
            //args.CopyTo(Variables, 0);
            for (var i = 0; i < args.Length; i++)
            {
                Variables[i] = args[i];
                NamedIdenificators[method.ParamNames[i]] = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillArguments(Method method, object[] args)
        {
            if (Variables.Length <= args.Length) Variables = new SLTSpeedyObject[8 + args.Length + Variables.Length];
            //args.CopyTo(Variables, 0);
            for (var i = 0; i < args.Length; i++)
            {
                Variables[i] = SLTSpeedyObject.GetAny(args[i]);
                NamedIdenificators[method.ParamNames[i]] = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SetValue(string name, object value)
        {
            if (NamedIdenificators.TryGetValue(name, out var id))
            {
                Variables[id] = SLTSpeedyObject.GetAny(value);
                return id;
            }
            else
            {
                if (NamedIdenificators.Count >= Variables.Length) Expand();
                var count = NamedIdenificators.Count;
                Variables[count] = SLTSpeedyObject.GetAny(value);
                NamedIdenificators[name] = count;
                return count;
            }
        }
        public int SetValue(string name, SLTSpeedyObject value)
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
        private void SetValue(int index, object value)
        {
            Variables[index] = value.ToSpeedy();
        }
        public void SetValue(int index, SLTSpeedyObject value)
        {
            Variables[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SLTSpeedyObject, int) GetValue(string name)
        {
            if (NamedIdenificators.TryGetValue(name, out var x))
            {
                return (Variables[x], x);
            }
            return (default, -1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SLTSpeedyObject GetValue(int index)
        {
            return Variables[index];
        }

        public LocalVariablesContainer()
        {
            
        }
    }

    public struct SLTSpeedyObject
    {
        public int Type;
        public bool AsBool;
        public double AsDouble;
        public ulong AsULong;
        public long AsLong;
        public object Any;

        public const int BoolType = 0;
        public const int DoubleType = 1;
        public const int ULongType = 2;
        public const int LongType = 3;
        public const int AnyType = 4;

        private static unsafe SLTSpeedyObject* get_ptr()
        {
            // Чтобы запустить эту реализацию, закомментируйте этот метод
            // Что касается попытки написать единую структуру, то есть следующие выводы:
            // FieldOffset не работает, если хотя бы один тип управляемый, значит невозможно использовать отступ
            // Также не работает unsafe с управляемыми типами. Единственный нормальный вариант - ref struct, то есть буквально указатель на структуру
            // Здесь этот вариант и реализован в надежде получить хоть какой-то прирост скорости
            // Но он дал ухудшение производительности в два раза!
            // пускай эта мертвая ветка лежит тут как памятник потерянным двум часам времени
            return null;
        }

        public override string ToString()
        {
            return Boxed().ToString();
        }

        public static SLTSpeedyObject GetBool(bool b)
        {
            return new SLTSpeedyObject() { Type = BoolType, AsBool = b };
        }


        public static SLTSpeedyObject GetDouble(double d)
        {
            return new SLTSpeedyObject() { Type = DoubleType, AsDouble = d };
        }

        public static SLTSpeedyObject GetULong(ulong u) 
        {
            return new SLTSpeedyObject() { Type = ULongType, AsULong = u };
        }

        public static SLTSpeedyObject GetLong(long l)
        {
            return new SLTSpeedyObject() { Type = LongType, AsLong = l };
        }

        public static SLTSpeedyObject GetAny(object o)
        {
            if (o is SLTSpeedyObject spd) return spd;
            else if (o is double d) return GetDouble(d);
            else if (o is ulong u) return GetULong(u);
            else if (o is long l) return GetLong(l);
            else if (o is bool b) return GetBool(b);
            return new SLTSpeedyObject() { Type = AnyType, Any = o };
        }

        public object Boxed()
        {
            if (Type == 0) return AsBool;
            else if (Type == 1) return AsDouble;
            else if (Type == 2) return AsULong;
            else if (Type == 3) return AsLong;
            else if (Type == 4) return Any;
            return null;
        }

        public object Boxed(SourceContext context)
        {
            if (Type < 0 || Type > 4) throw new RuntimeError("Trying unbox undefined type", context);
            if (Type == 0) return AsBool;
            else if (Type == 1) return AsDouble;
            else if (Type == 2) return AsULong;
            else if (Type == 3) return AsLong;
            else if (Type == 4) return Any;
            return null;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExecutionContext PrepareToInvoke() { Returned = Broken = Continued = false; return this; }
        public void DefaultEnvironment()
        {
            
        }

        public BaseStatement parse(string s) => new Parser().ParseScript(s);
        public object eval(IExecutable executable) => executable.GetValue(this);

        public LocalVariablesContainer LocalVariables { get; set; } = new LocalVariablesContainer();
    }
}
