using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace SLThree.Extensions
{
    public static class SLTHelpers
    {
#pragma warning disable IDE1006 // Стили именования
        public class random
        {
            public static object choose(IChooser chooser) => chooser.Choose();
            public static object to_chooser(object o)
            {
                if (o is IDictionary dictionary)
                {
                    var lst = new List<(object, double)>();
                    foreach (var key in dictionary.Keys)
                        lst.Add((key, dictionary[key].Cast<double>()));
                    return new ChanceChooser<object>(lst);
                }
                if (o is CreatorRange.RangeEnumerator enumerator) return new RangeChooser(enumerator.Lower, enumerator.Upper);
                if (o is double d) return new BooleanChooser(d);
                if (o is IEnumerable enumerable) return new EqualchanceChooser<object>(enumerable.Enumerate().ToArray());
                throw new ArgumentException($"{o?.GetType().GetTypeString() ?? "null"} is not convertable to chooser");
            }
            public static object to_chooser(object o, Type type)
            {
                if (o is IDictionary dictionary) return internal_chooser1reflected(dictionary, type);
                if (o is IEnumerable enumerable) return internal_chooser2reflected(enumerable, type);
                throw new ArgumentException($"{o?.GetType().GetTypeString() ?? "null"} is not convertable to chooser<{type.GetTypeString()}>");
            }

            private static ChanceChooser<T> internal_chooser1<T>(IDictionary dictionary)
            {
                var lst = new List<(T, double)>();
                foreach (var key in dictionary.Keys)
                    lst.Add((key.CastToType<T>(), dictionary[key].Cast<double>()));
                return new ChanceChooser<T>(lst);
            }
            private static EqualchanceChooser<T> internal_chooser2<T>(IEnumerable enumerable)
            {
                var ret = new List<T>();
                foreach (var x in enumerable)
                    ret.Add(x.CastToType<T>());
                return new EqualchanceChooser<T>(ret);
            }
            private static readonly MethodInfo i_c1 = ((Func<IDictionary, ChanceChooser<int>>)internal_chooser1<int>).Method.GetGenericMethodDefinition();
            private static readonly MethodInfo i_c2 = ((Func<IDictionary, EqualchanceChooser<int>>)internal_chooser2<int>).Method.GetGenericMethodDefinition();
            private static object internal_chooser1reflected(IDictionary dictionary, Type type)
                => i_c1.MakeGenericMethod(new Type[1] { type }).Invoke(null, new object[1] { dictionary });
            private static object internal_chooser2reflected(IEnumerable enumerable, Type type)
                => i_c2.MakeGenericMethod(new Type[1] { type }).Invoke(null, new object[1] { enumerable });
        }
#pragma warning restore IDE1006 // Стили именования
        
        /// <summary>
        /// МНЕ НАДОЕЛО ЭТИ СВИТЧИ ВРУЧНУЮ ПИСАТЬ!!!
        /// Метод, автоматически строящий соответствия 
        /// switch (T) {
        ///     case TA x: Method(x); break;
        ///     case TB x: Method(x); break;
        /// }
        /// Проверит наличие перегрузки
        /// </summary>
        /// <typeparam name="T">Класс, методы которого будут</typeparam>
        /// <returns></returns>
        public static Action<Target, T> CreateInheritorSwitcher<Target, T>(string methodName, Type[] excludedTypes, Type[] excludedInheritors)
        {
            bool IsInheritor(Type baseType, Type searchType)
            {
                var target = baseType;
                if (excludedTypes.Contains(target)) return false;
                target = target.BaseType;
                while (target != null)
                {
                    if (excludedTypes.Contains(target)) return false;
                    if (target == searchType) return true;
                    if (excludedInheritors.Contains(target)) return false;
                    target = target.BaseType;
                }
                return false;
            }

            var type = typeof(T);
            var assembly = type.Assembly;
            var inheritors = assembly.GetTypes()
                .Where(x => IsInheritor(x, type))
                .Select(x => new ValueTuple<Type, MethodInfo, Label>(x, null, default)).ToArray();
            var methods = typeof(Target).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < inheritors.Length; i++)
            {
                var m = methods.FirstOrDefault(x => x.Name == methodName && x.GetParameters()[0].ParameterType == inheritors[i].Item1);
                if (m == null) throw new Exception($"Не найден перегруженный метод {methodName}({inheritors[i].Item1})");
                inheritors[i].Item2 = m;
            }

            var method = new DynamicMethod("GENERATED", typeof(void), new Type[2] { typeof(Target), type });
            var il = method.GetILGenerator();

            for (var i = 0; i < inheritors.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Isinst, inheritors[i].Item1);
                inheritors[i].Item3 = il.DefineLabel();
                il.Emit(OpCodes.Brtrue, inheritors[i].Item3);
            }
            il.Emit(OpCodes.Ret);
            for (var i = 0; i < inheritors.Length; i++)
            {
                il.MarkLabel(inheritors[i].Item3);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Isinst, inheritors[i].Item1);
                il.Emit(OpCodes.Callvirt, inheritors[i].Item2);
                il.Emit(OpCodes.Ret);
            }
            return (Action<Target, T>)method.CreateDelegate(typeof(Action<Target, T>));
        }

        public static bool IsAbstract(this StatementList statement)
        {
            return statement.Statements.Length > 0
                && statement.Statements[0] is ThrowStatement @throw
                && @throw.ThrowExpression is ObjectLiteral expression
                && expression.Value is AbstractInvokation;
        }

        public static object CastToMax(this object o)
        {
            switch (o)
            {
                case char v: return (long)v;
                case float v: return (double)v;
                case sbyte v: return (long)v;
                case short v: return (long)v;
                case int v: return (long)v;
                case byte v: return (ulong)v;
                case ushort v: return (ulong)v;
                case uint v: return (ulong)v;
            }
            return o;
        }

        public static string GetTypeString(this Type t)
        {
            if (t == type_long) return "i64";
            if (t == type_double) return "f64";
            if (t == type_ulong) return "u64";
            if (t == type_queue) return "queue";
            if (t == type_stack) return "stack";
            if (t == type_list) return "list";
            if (t == type_dict) return "dict";
            if (t == type_array) return "array";
            if (t.IsArray)
                return t.GetArrayRank() == 1 ? $"array<{t.GetElementType().GetTypeString()}>" : $"array{t.GetArrayRank()}<{t.GetElementType().GetTypeString()}>";
            if (t.IsGenericTypeDefinition)
            {
                var ct = t.GetGenericArguments().Length;
                return $"{t.FullName.Substring(0, t.FullName.IndexOf('`')).Split('.').Last()}<{(ct > 1 ? new string(',', ct) : "")}>";
            }
            if (t.IsGenericType)
            {
                var generic_def = t.GetGenericTypeDefinition();
                var name = string.Empty;
                if (generic_def == type_generic_list) name = "list";
                else if (generic_def == type_generic_dict) name = "dict";
                else if (generic_def == type_generic_stack) name = "stack";
                else if (generic_def == type_generic_queue) name = "queue";
                else if (t.Name.StartsWith("ValueTuple")) name = "tuple";
                else if (t.FullName != null) name = t.FullName.Substring(0, t.FullName.IndexOf('`')).Split('.').Last();
                else name = t.Name.Substring(0, t.Name.IndexOf('`')).Split('.').Last();
                return $"{name}<{t.GetGenericArguments().ConvertAll(x => x.GetTypeString()).JoinIntoString(", ")}>";
            }
            if (t == type_object) return "any";
            if (t == type_byte) return "u8";
            if (t == type_sbyte) return "i8";
            if (t == type_short) return "i16";
            if (t == type_ushort) return "u16";
            if (t == type_int) return "i32";
            if (t == type_uint) return "u32";
            if (t == type_float) return "f32";
            if (t == type_bool) return "bool";
            if (t == type_string) return "string";
            if (t == type_char) return "char";
            if (t == type_context) return "context";
            if (t == type_void) return "void";
            if (t.FullName == null) return t.Name;

            else return t.FullName;
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == type_generic_list;
        }
        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == type_generic_dict;
        }
        public static bool IsStack(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == type_generic_stack;
        }
        public static bool IsQueue(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == type_generic_queue;
        }
        private static Dictionary<Type, bool> is_tuple_cache = new Dictionary<Type, bool>()
        {
            { typeof(ITuple), true },
        };
        public static bool IsTuple(this Type type)
            => is_tuple_cache.TryGetValue(type, out var result)
            ? result
            : (is_tuple_cache[type] = type.GetInterfaces().Contains(type_tuple));
        private static Type type_generic_list = typeof(List<>);
        private static Type type_generic_stack = typeof(Stack<>);
        private static Type type_generic_queue = typeof(Queue<>);
        private static Type type_generic_dict = typeof(Dictionary<,>);
        private static Type type_void = typeof(void);
        public static Type ToType(this string s, bool throwError = false)
        {
            switch (s)
            {
                case "i64": return type_long;
                case "f64": return type_double;
                case "u64": return type_ulong;
                case "string": return type_string;
                case "i32": return type_int;
                case "f32": return type_float;
                case "u8": return type_byte;
                case "i8": return type_sbyte;
                case "i16": return type_short;
                case "u16": return type_ushort;
                case "u32": return type_uint;
                case "any": return type_object;
                case "bool": return type_bool;
                case "char": return type_char;
                case "context": return type_context;
                case "list": return type_list;
                case "list`1": return type_generic_list;
                case "stack": return type_stack;
                case "stack`1": return type_generic_stack;
                case "queue": return type_queue;
                case "queue`1": return type_generic_queue;
                case "dict": return type_dict;
                case "dict`2": return type_generic_dict;
                case "tuple`1": return typeof(ValueTuple<>);
                case "tuple`2": return typeof(ValueTuple<,>);
                case "tuple`3": return typeof(ValueTuple<,,>);
                case "tuple`4": return typeof(ValueTuple<,,,>);
                case "tuple`5": return typeof(ValueTuple<,,,,>);
                case "tuple`6": return typeof(ValueTuple<,,,,,>);
                case "tuple`7": return typeof(ValueTuple<,,,,,,>);
                case "tuple`8": return typeof(ValueTuple<,,,,,,,>);
                case "array": return type_array;
            }
            return GetTypeFromRegistredAssemblies(s, throwError);
        }
        private static readonly Dictionary<string, Type> founded_types = new Dictionary<string, Type>();
        public static Type GetTypeFromRegistredAssemblies(this string s, bool throwError)
        {
            if (founded_types.TryGetValue(s, out var type)) return type;
            else
            {
                foreach (var ass in DotnetEnvironment.RegistredAssemblies)
                {
                    var ret = ass.GetType(s, throwError);
                    if (ret != null) return founded_types[s] = ret;
                    else
                    {
                        foreach (var str in s.NestedVariations())
                        {
                            ret = ass.GetType(str, throwError);
                            if (ret != null) return founded_types[s] = ret;
                        }
                    }
                }
            }
            if (throwError) throw new Exception($"Type {s} not found");
            return null;
        }
        private static IEnumerable<string> NestedVariations(this string s)
        {
            int ind;
            while ((ind = s.LastIndexOf('.')) != -1)
            {
                yield return s = s.Substring(0, ind) + "+" + s.Substring(ind + 1, s.Length - ind - 1);
            }
        }
        private static Type 
            type_bool = typeof(bool),
            type_string = typeof(string),
            type_object = typeof(object),

            type_byte = typeof(byte),
            type_sbyte = typeof(sbyte),
            type_ushort = typeof(ushort),
            type_short = typeof(short),
            type_uint = typeof(uint),
            type_int = typeof(int),
            type_ulong = typeof(ulong),
            type_long = typeof(long),
            type_char = typeof(char),
            type_double = typeof(double),
            type_float = typeof(float),
            type_context = typeof(ContextWrap),

            type_array = typeof(object[]),
            type_list = typeof(List<object>),
            type_dict = typeof(Dictionary<object, object>),
            type_stack = typeof(Stack<object>),
            type_queue = typeof(Queue<object>),
            type_tuple = typeof(ITuple);

        public static BaseExpression RaisePriority(this BaseExpression expression)
        {
            expression.PrioriryRaised = true;
            return expression;
        }

        public static BaseExpression DropPriority(this BaseExpression expression)
        {
            expression.PrioriryRaised = false;
            return expression;
        }

        public static T CastToType<T>(this object o)
        {
            return (T)o.CastToType(typeof(T));
        }

        public static object CastToType(this object o, Type casting_type)
        {
            if (o == null) return null;

            if (casting_type == typeof(string))
                return o.ToString();
            var type = o.GetType();
            if (type == casting_type) return o;
            if (casting_type.IsEnum)
            {
                if (type == type_string) return Enum.Parse(casting_type, (string)o);
                if (type == type_int) return Enum.ToObject(casting_type, (int)o);
                if (type == type_byte) return Enum.ToObject(casting_type, (byte)o);
                if (type == type_sbyte) return Enum.ToObject(casting_type, (sbyte)o);
                if (type == type_ushort) return Enum.ToObject(casting_type, (ushort)o);
                if (type == type_short) return Enum.ToObject(casting_type, (short)o);
                if (type == type_uint) return Enum.ToObject(casting_type, (uint)o);
                if (type == type_ulong) return Enum.ToObject(casting_type, (ulong)o);
                if (type == type_long) return Enum.ToObject(casting_type, (long)o);
            }
            if (o is IConvertible) return Convert.ChangeType(o, casting_type);
            return o;
        }

        public static bool IsType(this Type type, Type other)
        {
            if (type == other) return true;
            if (other.IsAssignableFrom(type)) return true;
            return false;
        }
    }
}
