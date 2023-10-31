using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public static class Wrapper<T> where T : new()
    {
        private static Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();
        private static Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        private static PropertyInfo InjectClassname = null;
        #region Type Setting
        static Wrapper()
        {
            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var property in props)
            {
                if (Attribute.IsDefined(property, typeof(WrapperSkipAttribute))) continue;
                else if (property.SetMethod == null) continue;
                //else if (Attribute.IsDefined(property, typeof(WrappingInjectClassname))) InjectClassname = property;
                else Properties[property.Name] = property;
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(WrapperSkipAttribute))) continue;
                else Fields[field.Name] = field;
            }
        }
        private static Type generic_list = typeof(List<int>).GetGenericTypeDefinition();
        private static Type generic_dict = typeof(Dictionary<string, int>).GetGenericTypeDefinition();
        private static Type type_ituple = typeof(ITuple);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Пераметр NET-типа</param>
        /// <param name="b">Параметр BQS-типа</param>
        /// <returns></returns>
        private static bool HasRecast(Type type)
        {
            if (type.IsArray) return true;
            if (type.IsGenericType)
            {
                var gen_a = type.GetGenericTypeDefinition();
                if (type == generic_list) return true;
                if (type == generic_dict) return true;
            }
            var interfaces = type.GetInterfaces();
            if (interfaces.Contains(type_ituple)) return true;
            return false;
        }
        private static object[] TupleToArray(ITuple tuple)
        {
            var ret = new object[tuple.Length];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = tuple[i];
            return ret;
        }
        private static object Cast(Type type_to_cast, object o)
        {
            if (type_to_cast.IsArray && o is object[] obj_array)
            {
                var gga = type_to_cast.GetElementType();
                var ret = Array.CreateInstance(gga, obj_array.Length);
                if (HasRecast(gga))
                {
                    for (var i = 0; i < obj_array.Length; i++)
                        ret.SetValue(Cast(gga, obj_array[i]), i);
                }
                else
                {
                    for (var i = 0; i < obj_array.Length; i++)
                        ret.SetValue(obj_array[i], i);
                }
                return ret;
            }
            else if (type_to_cast.IsArray && o is List<object> obj_list)
            {
                var gga = type_to_cast.GetElementType();
                var ret = Array.CreateInstance(gga, obj_list.Count);
                if (HasRecast(gga))
                {
                    for (var i = 0; i < obj_list.Count; i++)
                        ret.SetValue(Cast(gga, obj_list[i]), i);
                }
                else
                {
                    for (var i = 0; i < obj_list.Count; i++)
                        ret.SetValue(obj_list[i], i);
                }
                return ret;
            }
            if (type_to_cast.IsGenericType)
            {
                var gen_type = type_to_cast.GetGenericTypeDefinition();
                if (gen_type == generic_list && o is List<object> obj_list)
                {
                    var gga = type_to_cast.GetGenericArguments();
                    var ret = type_to_cast.GetConstructor(new Type[0]).Invoke(new object[0]) as IList;
                    if (HasRecast(gga[0]))
                    {
                        for (var i = 0; i < obj_list.Count; i++)
                            ret.Add(Cast(gga[0], obj_list[i]));
                    }
                    else
                    {
                        for (var i = 0; i < obj_list.Count; i++)
                            ret.Add(obj_list[i]);
                    }
                    return ret;
                }
                if (gen_type == generic_dict && o is Dictionary<object, object> obj_dict)
                {
                    var gga = type_to_cast.GetGenericArguments();
                    var ret = type_to_cast.GetConstructor(new Type[0]).Invoke(new object[0]) as IDictionary;
                    if (HasRecast(gga[0]) || HasRecast(gga[1]))
                    {
                        foreach (var x in obj_dict)
                            ret[Cast(gga[0], x.Key)] = Cast(gga[1], x.Value);
                    }
                    else
                    {
                        foreach (var x in obj_dict)
                            ret[x.Key] = x.Value;
                    }
                    return ret;
                }
            }
            var interfaces = type_to_cast.GetInterfaces();
            if (interfaces.Contains(type_ituple) && o is ITuple tuple)
            {
                var gga = type_to_cast.GetGenericArguments();
                if (gga.Any(x => HasRecast(x)))
                {
                    var ret = type_to_cast.GetConstructor(gga).Invoke(TupleToArray(tuple).Select((x, i) => Cast(gga[0], x)).ToArray());
                    return ret;
                }
                else
                {
                    var ret = type_to_cast.GetConstructor(gga).Invoke(TupleToArray(tuple));
                    return ret;
                }
            }
            return o;
        }
        #endregion
        public static T Unwrap(ExecutionContext context)
        {
            var ret = new T();
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                if (Properties.ContainsKey(name.Key)) Properties[name.Key].SetValue(ret, Cast(Properties[name.Key].PropertyType, name.Value));
                else if (Fields.ContainsKey(name.Key)) Fields[name.Key].SetValue(ret, Cast(Fields[name.Key].FieldType, name.Value));
            }
            return ret;
        }
    }

    /// <summary>
    /// Это свойство будет пропущено при разворачивании
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class WrapperSkipAttribute : Attribute
    {
        public WrapperSkipAttribute() { }
    }

/*    /// <summary>
    /// В это свойство будет вписано имя класса
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class WrappingInjectClassname : Attribute
    {
        public WrappingInjectClassname() { }
    }*/
}
