using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                case bool v: return v ? 1 : (long)0;
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

        public static SLTSpeedyObject ToSpeedy(this object o)
        {
            return SLTSpeedyObject.GetAny(o);
        }
        public static ref SLTSpeedyObject ToSpeedy(this object o, ref SLTSpeedyObject reference)
        {
            reference = SLTSpeedyObject.GetAny(o);
            return ref reference;
        }
        public static string GetTypeString(this Type t)
        {
            if (t == type_object) return "object";
            if (t == type_byte) return "u8";
            if (t == type_sbyte) return "i8";
            if (t == type_short) return "i16";
            if (t == type_ushort) return "u16";
            if (t == type_int) return "i32";
            if (t == type_uint) return "u32";
            if (t == type_long) return "i64";
            if (t == type_ulong) return "u64";
            if (t == type_double) return "f64";
            if (t == type_float) return "f32";
            if (t == type_bool) return "bool";
            if (t == type_string) return "string";
            if (t == type_char) return "char";

            else return t.FullName;
        }
        public static string GetTypeString(this string t)
        {
            if (t == "System.Object") return "object";
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
            else return t;
        }
        public static Type ToType(this string s)
        {
            switch (s)
            {
                case "u8": return type_byte;
                case "i8": return type_sbyte;
                case "i16": return type_short;
                case "u16": return type_ushort;
                case "i32": return type_int;
                case "u32": return type_uint;
                case "i64": return type_long;
                case "u64": return type_ulong;
                case "f64": return type_double;
                case "f32": return type_float;
                case "string": return type_string;
                case "object": return type_object;
                case "bool": return type_bool;
                case "char": return type_char;
            }
            return Type.GetType(s, false);
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
        public static object CastToType(this object o, Type casting_type)
        {
            if (o == null) return null;
            if (casting_type == typeof(string))
            {
                return o.ToString();
            }
            var type = o.GetType();
            if (casting_type.IsEnum)
            {
                if (type == type_byte) return Enum.ToObject(casting_type, (byte)o);
                if (type == type_sbyte) return Enum.ToObject(casting_type, (sbyte)o);
                if (type == type_ushort) return Enum.ToObject(casting_type, (ushort)o);
                if (type == type_short) return Enum.ToObject(casting_type, (short)o);
                if (type == type_uint) return Enum.ToObject(casting_type, (uint)o);
                if (type == type_int) return Enum.ToObject(casting_type, (int)o);
                if (type == type_ulong) return Enum.ToObject(casting_type, (ulong)o);
                if (type == type_long) return Enum.ToObject(casting_type, (long)o);
                if (type == type_string) return Enum.Parse(casting_type, (string)o);
            }
            if (type.IsEnum)
            {
                if (casting_type == type_byte) return (byte)o;
                if (casting_type == type_sbyte) return (sbyte)o;
                if (casting_type == type_ushort) return (ushort)o;
                if (casting_type == type_short) return (short)o;
                if (casting_type == type_uint) return (uint)o;
                if (casting_type == type_int) return (int)o;
                if (casting_type == type_ulong) return (ulong)o;
                if (casting_type == type_long) return (long)o;
            }
            if (casting_type == type_char)
            {
                switch (o)
                {
                    case byte b: return (char)b;
                    case sbyte b: return (char)b;
                    case ushort b: return (char)b;
                    case short b: return (char)b;
                    case uint b: return (char)b;
                    case int b: return (char)b;
                    case ulong b: return (char)b;
                    case long b: return (char)b;
                    case float b: return (char)b;
                    case double b: return (char)b;
                    case string b_str:
                        {
                            if (string.IsNullOrEmpty(b_str)) return (char)0;
                            else return b_str[0];
                        }
                }
            }
            else if (casting_type == type_byte)
            {
                switch (o)
                {
                    case byte b: return b;
                    case sbyte b: return (byte)b;
                    case ushort b: return (byte)b;
                    case short b: return (byte)b;
                    case uint b: return (byte)b;
                    case int b: return (byte)b;
                    case ulong b: return (byte)b;
                    case long b: return (byte)b;
                    case float b: return (byte)b;
                    case double b: return (byte)b;
                    case string b_str:
                        {
                            return byte.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_sbyte)
            {
                switch (o)
                {
                    case byte b: return (sbyte)b;
                    case sbyte b: return b;
                    case ushort b: return (sbyte)b;
                    case short b: return (sbyte)b;
                    case uint b: return (sbyte)b;
                    case int b: return (sbyte)b;
                    case ulong b: return (sbyte)b;
                    case long b: return (sbyte)b;
                    case float b: return (sbyte)b;
                    case double b: return (sbyte)b;
                    case string b_str:
                        {
                            return sbyte.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_ushort)
            {
                switch (o)
                {
                    case byte b: return (ushort)b;
                    case sbyte b: return (ushort)b;
                    case ushort b: return b;
                    case short b: return (ushort)b;
                    case uint b: return (ushort)b;
                    case int b: return (ushort)b;
                    case ulong b: return (ushort)b;
                    case long b: return (ushort)b;
                    case float b: return (ushort)b;
                    case double b: return (ushort)b;
                    case string b_str:
                        {
                            return ushort.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_short)
            {
                switch (o)
                {
                    case byte b: return (short)b;
                    case sbyte b: return (short)b;
                    case ushort b: return (short)b;
                    case short b: return b;
                    case uint b: return (short)b;
                    case int b: return (short)b;
                    case ulong b: return (short)b;
                    case long b: return (short)b;
                    case float b: return (short)b;
                    case double b: return (short)b;
                    case string b_str:
                        {
                            return short.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_uint)
            {
                switch (o)
                {
                    case byte b: return (uint)b;
                    case sbyte b: return (uint)b;
                    case ushort b: return (uint)b;
                    case short b: return (uint)b;
                    case uint b: return b;
                    case int b: return (uint)b;
                    case ulong b: return (uint)b;
                    case long b: return (uint)b;
                    case float b: return (uint)b;
                    case double b: return (uint)b;
                    case string b_str:
                        {
                            return uint.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_int)
            {
                switch (o)
                {
                    case byte b: return (int)b;
                    case sbyte b: return (int)b;
                    case ushort b: return (int)b;
                    case short b: return (int)b;
                    case uint b: return (int)b;
                    case int b: return b;
                    case ulong b: return (int)b;
                    case long b: return (int)b;
                    case float b: return (int)b;
                    case double b: return (int)b;
                    case string b_str:
                        {
                            return int.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_ulong)
            {
                switch (o)
                {
                    case byte b: return (ulong)b;
                    case sbyte b: return (ulong)b;
                    case ushort b: return (ulong)b;
                    case short b: return (ulong)b;
                    case uint b: return (ulong)b;
                    case int b: return (ulong)b;
                    case ulong b: return b;
                    case long b: return (ulong)b;
                    case float b: return (ulong)b;
                    case double b: return (ulong)b;
                    case string b_str:
                        {
                            return ulong.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_long)
            {
                switch (o)
                {
                    case byte b: return (long)b;
                    case sbyte b: return (long)b;
                    case ushort b: return (long)b;
                    case short b: return (long)b;
                    case uint b: return (long)b;
                    case int b: return (long)b;
                    case ulong b: return (long)b;
                    case long b: return b;
                    case float b: return (long)b;
                    case double b: return (long)b;
                    case string b_str:
                        {
                            return long.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_float)
            {
                switch (o)
                {
                    case byte b: return (float)b;
                    case sbyte b: return (float)b;
                    case ushort b: return (float)b;
                    case short b: return (float)b;
                    case uint b: return (float)b;
                    case int b: return (float)b;
                    case ulong b: return (float)b;
                    case long b: return (float)b;
                    case float b: return b;
                    case double b: return (float)b;
                    case string b_str:
                        {
                            return float.Parse(b_str);
                        }
                }
            }
            else if (casting_type == type_double)
            {
                switch (o)
                {
                    case byte b: return (double)b;
                    case sbyte b: return (double)b;
                    case ushort b: return (double)b;
                    case short b: return (double)b;
                    case uint b: return (double)b;
                    case int b: return (double)b;
                    case ulong b: return (double)b;
                    case long b: return (double)b;
                    case float b: return (double)b;
                    case double b: return b;
                    case string b_str:
                        {
                            return double.Parse(b_str);
                        }
                }
            }
            return o;
        }

        public static long ToLong(this bool b) => b ? 1 : 0;
    }
}
