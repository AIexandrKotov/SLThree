using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Extensions
{
    public static class SLTHelpers
    {
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
                else if (t.Name.StartsWith("ValueTuple")) name = "tuple";
                else name = t.FullName.Substring(0, t.FullName.IndexOf('`')).Split('.').Last();
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
        private static Dictionary<Type, bool> is_tuple_cache = new Dictionary<Type, bool>()
        {
            { typeof(ITuple), true },
        };
        public static bool IsTuple(this Type type)
            => is_tuple_cache.TryGetValue(type, out var result)
            ? result
            : (is_tuple_cache[type] = type.GetInterfaces().Contains(type_tuple));
        private static Type type_generic_list = typeof(List<>);
        private static Type type_generic_dict = typeof(Dictionary<,>);
        private static Type type_void = typeof(void);

        /// <summary>
        /// Only for compatibility with old REPLs
        /// </summary>
        public static string GetTypeString(this string t)
        {
            if (t == "System.Object") return "any";
            if (t == "System.Byte") return "u8";
            if (t == "System.SByte") return "i8";
            if (t == "System.Int16") return "i16";
            if (t == "System.UInt16") return "u16";
            if (t == "System.Int32") return "i32";
            if (t == "System.UInt32") return "u32";
            if (t == "System.Int64") return "i64";
            if (t == "System.UInt64") return "u64";
            if (t == "System.Double") return "f64";
            if (t == "System.Single") return "f32";
            if (t == "System.Boolean") return "bool";
            if (t == "System.String") return "string";
            if (t == "System.Char") return "char";
            if (t == "SLThree.ExecutionContext+ContextWrap") return "context";
            if (t == "System.Void") return "void";
            else return t;
        }
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
                foreach (var ass in sys.slt.registred)
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
            var ind = -1;
            while ((ind = s.LastIndexOf('.')) != -1)
            {
                yield return s = s.Substring(0, ind) + "+" + s.Substring(ind + 1, s.Length - ind - 1);
            }
        }
        private static Type type_bool = typeof(bool);
        private static Type type_string = typeof(string);
        private static Type type_object = typeof(object);

        private static Type type_byte = typeof(byte);
        private static Type type_sbyte = typeof(sbyte);
        private static Type type_ushort = typeof(ushort);
        private static Type type_short = typeof(short);
        private static Type type_uint = typeof(uint);
        private static Type type_int = typeof(int);
        private static Type type_ulong = typeof(ulong);
        private static Type type_long = typeof(long);
        private static Type type_char = typeof(char);
        private static Type type_double = typeof(double);
        private static Type type_float = typeof(float);
        private static Type type_context = typeof(ExecutionContext.ContextWrap);

        private static Type type_array = typeof(object[]);
        private static Type type_list = typeof(List<object>);
        private static Type type_dict = typeof(Dictionary<object, object>);
        private static Type type_tuple = typeof(ITuple);

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

        public static object CastToType(this object o, Type casting_type)
        {
            if (o == null) return null;

            if (casting_type == typeof(string))
                return o.ToString();
            if (casting_type == type_context)
            {
                if (o is Type st_type)
                    return 
                        st_type.IsAbstract && st_type.IsSealed
                        ? new ExecutionContext.ContextWrap(NonGenericWrapper.GetWrapper(st_type).WrapStaticClass())
                        : new ExecutionContext.ContextWrap(NonGenericWrapper.GetWrapper(st_type).WrapStatic());
                else
                    return new ExecutionContext.ContextWrap(NonGenericWrapper.GetWrapper(o.GetType()).Wrap(o));
            }
            if (o is IConvertible) return Convert.ChangeType(o, casting_type);
            var type = o.GetType();
            if (type == type_context)
            {
                var wrapper = NonGenericWrapper.GetWrapper(casting_type);
                if (casting_type.IsAbstract)
                {
                    if (casting_type.IsSealed) wrapper.UnwrapStaticClass(((ExecutionContext.ContextWrap)o).pred);
                    else wrapper.UnwrapStatic(((ExecutionContext.ContextWrap)o).pred);
                    return null;
                }
                else return wrapper.Unwrap(((ExecutionContext.ContextWrap)o).pred);
            }
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
