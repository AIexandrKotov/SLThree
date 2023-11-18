using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class WrappersTypeSetting
    {
        #region Type Setting
        protected static Type generic_list = typeof(List<int>).GetGenericTypeDefinition();
        protected static Type generic_dict = typeof(Dictionary<string, int>).GetGenericTypeDefinition();
        protected static Type type_ituple = typeof(ITuple);
        protected static bool HasRecast(Type type)
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
        protected static object[] TupleToArray(ITuple tuple)
        {
            var ret = new object[tuple.Length];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = tuple[i];
            return ret;
        }
        protected static object UnwrapCast(Type type_to_cast, object o)
        {
            if (type_to_cast.IsArray && o is object[] obj_array)
            {
                var gga = type_to_cast.GetElementType();
                var ret = Array.CreateInstance(gga, obj_array.Length);
                if (HasRecast(gga))
                {
                    for (var i = 0; i < obj_array.Length; i++)
                        ret.SetValue(UnwrapCast(gga, obj_array[i]), i);
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
                        ret.SetValue(UnwrapCast(gga, obj_list[i]), i);
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
                            ret.Add(UnwrapCast(gga[0], obj_list[i]));
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
                            ret[UnwrapCast(gga[0], x.Key)] = UnwrapCast(gga[1], x.Value);
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
                    var ret = type_to_cast.GetConstructor(gga).Invoke(TupleToArray(tuple).Select((x, i) => UnwrapCast(gga[0], x)).ToArray());
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
        protected static object WrapCast(object o)
        {
            if (o == null) return null;
            var type = o.GetType();
            if (type.IsArray)
            {
                var arr = o as Array;
                var ret = new object[arr.Length];
                if (HasRecast(type.GetElementType()))
                {
                    for (var i = 0; i < arr.Length; i++)
                        ret[i] = WrapCast(arr.GetValue(i));
                }
                else
                {
                    for (var i = 0; i < arr.Length; i++)
                        ret[i] = arr.GetValue(i);
                }
                return ret;
            }
            else if (type.IsGenericType)
            {
                var gt = type.GetGenericTypeDefinition();
                if (gt == generic_list)
                {
                    var lst = o as IList;
                    var ret = new List<object>(lst.Count);
                    var gga = gt.GetGenericArguments();
                    if (HasRecast(gga[0]))
                    {
                        for (var i = 0; i < lst.Count; i++)
                            ret[i] = WrapCast(lst[i]);
                    }
                    else
                    {
                        for (var i = 0; i < lst.Count; i++)
                            ret[i] = lst[i];
                    }
                    return ret;

                }
                else if (gt == generic_dict)
                {
                    var dct = o as IDictionary;
                    var ret = new Dictionary<object, object>();
                    var gga = gt.GetGenericArguments();
                    if (HasRecast(gga[0]) || HasRecast(gga[1]))
                    {
                        foreach (var x in dct.Keys)
                            ret[WrapCast(x)] = WrapCast(dct[x]);
                    }
                    else
                    {
                        foreach (var x in dct.Keys)
                            ret[x] = dct[x];
                    }
                    return ret;
                }
            }
            else if (type.GetInterfaces().Contains(type_ituple))
            {
                //todo supporting any-size tuples
            }
            return o;
        }
        #endregion

        internal protected WrappersTypeSetting() { }
    }

    public class NonGenericWrapper : WrappersTypeSetting
    {
        protected Type type;
        protected string typename;
        protected int counter;
        internal string GetWrappedContextName() => $"<{typename}>@{Convert.ToString(counter++, 16).ToUpper().PadLeft(2, '0')}";
        private static Dictionary<Type, NonGenericWrapper> Wrappers { get; } = new Dictionary<Type, NonGenericWrapper>();
        public static NonGenericWrapper GetWrapper(Type type)
        {
            if (Wrappers.TryGetValue(type, out var value)) return value;
            else return new NonGenericWrapper(type);
        }
        public static NonGenericWrapper GetWrapper<T>() => GetWrapper(typeof(T));

        protected internal NonGenericWrapper() { }
        protected readonly Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();
        protected Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        protected readonly Dictionary<string, PropertyInfo> StaticProperties = new Dictionary<string, PropertyInfo>();
        protected readonly Dictionary<string, FieldInfo> StaticFields = new Dictionary<string, FieldInfo>();
        protected readonly PropertyInfo InjectContextName = null;
        protected readonly bool SupportedNameWrap;
        protected internal NonGenericWrapper(Type type)
        {
            this.type = type;
            Wrappers.Add(type, this);
            typename = type.Name;
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var property in props)
            {
                if (Attribute.IsDefined(property, typeof(WrapperContextNameAttribute))) InjectContextName = property;
                if (Attribute.IsDefined(property, typeof(WrapperSkipAttribute))) continue;
                else Properties[property.Name] = property;
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(WrapperSkipAttribute))) continue;
                else Fields[field.Name] = field;
            }
            var static_props = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var property in static_props)
            {
                if (Attribute.IsDefined(property, typeof(WrapperSkipAttribute))) continue;
                //else if (Attribute.IsDefined(property, typeof(WrappeContextNameAttribute))) InjectContextName = property;
                else StaticProperties[property.Name] = property;
            }
            var static_fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in static_fields)
            {
                if (Attribute.IsDefined(field, typeof(WrapperSkipAttribute))) continue;
                else StaticFields[field.Name] = field;
            }
            if (InjectContextName != null) SupportedNameWrap = InjectContextName.SetMethod != null;
        }

        public string GetName(object obj)
        {
            if (InjectContextName != null) return (string)InjectContextName.GetValue(obj);
            throw new ArgumentException($"Type {typename} doesn't have property with InjectContextName");
        }
        public ExecutionContext Wrap(object obj)
        {
            var ret = new ExecutionContext();
            ret.Name = SupportedNameWrap ? InjectContextName.GetValue(obj).ToString() : GetWrappedContextName();
            foreach (var x in Properties)
                ret.LocalVariables.SetValue(x.Key, WrapCast(x.Value.GetValue(obj)));
            foreach (var x in Fields)
                ret.LocalVariables.SetValue(x.Key, WrapCast(x.Value.GetValue(obj)));
            return ret;
        }
        public ExecutionContext WrapStatic()
        {
            var ret = new ExecutionContext();
            ret.Name = GetWrappedContextName();
            foreach (var x in StaticProperties)
                ret.LocalVariables.SetValue(x.Key, WrapCast(x.Value.GetValue(null)));
            foreach (var x in StaticFields)
                ret.LocalVariables.SetValue(x.Key, WrapCast(x.Value.GetValue(null)));
            return ret;
        }
        public void SafeUnwrapStatic(ExecutionContext context)
        {
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                try
                {
                    if (Properties.ContainsKey(name.Key) && Properties[name.Key].SetMethod != null)
                        Properties[name.Key].SetValue(null, UnwrapCast(Properties[name.Key].PropertyType, name.Value));
                    else if (Fields.ContainsKey(name.Key)) Fields[name.Key].SetValue(null, UnwrapCast(Fields[name.Key].FieldType, name.Value));
                }
                catch (Exception e)
                {
                    context.Errors.Add(e);
                }
            }
        }
        public void UnwrapStatic(ExecutionContext context)
        {
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                if (Properties.ContainsKey(name.Key) && Properties[name.Key].SetMethod != null)
                    Properties[name.Key].SetValue(null, UnwrapCast(Properties[name.Key].PropertyType, name.Value));
                else if (Fields.ContainsKey(name.Key)) Fields[name.Key].SetValue(null, UnwrapCast(Fields[name.Key].FieldType, name.Value));
            }
        }
        public ExecutionContext WrapStaticClass()
        {
            var ret = new ExecutionContext();
            ret.Name = GetWrappedContextName();
            var props = StaticProperties;
            var fields = StaticFields;
            foreach (var x in props)
                ret.LocalVariables.SetValue(x.Key, x.Value.GetValue(null));
            foreach (var x in fields)
                ret.LocalVariables.SetValue(x.Key, x.Value.GetValue(null));
            return ret;
        }
        public void UnwrapStaticClass(ExecutionContext context)
        {
            var props = StaticProperties;
            var fields = StaticFields;
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                if (props.ContainsKey(name.Key) && props[name.Key].SetMethod != null)
                    props[name.Key].SetValue(null, UnwrapCast(props[name.Key].PropertyType, name.Value));
                else if (fields.ContainsKey(name.Key)) fields[name.Key].SetValue(null, UnwrapCast(fields[name.Key].FieldType, name.Value));
            }
        }
        public void SafeUnwrapStaticClass(ExecutionContext context)
        {
            var props = StaticProperties;
            var fields = StaticFields;
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                try
                {
                    if (props.ContainsKey(name.Key) && props[name.Key].SetMethod != null)
                        props[name.Key].SetValue(null, UnwrapCast(props[name.Key].PropertyType, name.Value));
                    else if (fields.ContainsKey(name.Key)) fields[name.Key].SetValue(null, UnwrapCast(fields[name.Key].FieldType, name.Value));
                }
                catch (Exception e)
                {
                    context.Errors.Add(e);
                }
            }
        }
        public object Unwrap(ExecutionContext context)
        {
            var ret = Activator.CreateInstance(type);
            if (InjectContextName != null) InjectContextName.SetValue(ret, context.Name);
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                if (Properties.ContainsKey(name.Key) && Properties[name.Key].SetMethod != null)
                    Properties[name.Key].SetValue(ret, UnwrapCast(Properties[name.Key].PropertyType, name.Value));
                else if (Fields.ContainsKey(name.Key)) Fields[name.Key].SetValue(ret, UnwrapCast(Fields[name.Key].FieldType, name.Value));
            }
            return ret;
        }
        public object SafeUnwrap(ExecutionContext context)
        {
            var ret = Activator.CreateInstance(type);
            try
            {
                if (InjectContextName != null) InjectContextName.SetValue(ret, context.Name);
            }
            catch (Exception e)
            {
                context.Errors.Add(e);
            }
            foreach (var name in context.LocalVariables.GetAsDictionary())
            {
                try
                {
                    if (Properties.ContainsKey(name.Key) && Properties[name.Key].SetMethod != null)
                        Properties[name.Key].SetValue(ret, UnwrapCast(Properties[name.Key].PropertyType, name.Value));
                    else if (Fields.ContainsKey(name.Key)) Fields[name.Key].SetValue(ret, UnwrapCast(Fields[name.Key].FieldType, name.Value));
                }
                catch (Exception e)
                {
                    context.Errors.Add(e);
                }
            }
            return ret;
        }
    }

    public static class Wrapper<T>
    {
        private static readonly NonGenericWrapper wrapper;
        static Wrapper()
        {
            wrapper = NonGenericWrapper.GetWrapper(typeof(T));
        }
        public static T Unwrap(ExecutionContext context) => (T)wrapper.Unwrap(context);
        public static void UnwrapStatic(ExecutionContext context) => wrapper.UnwrapStatic(context);
        public static ExecutionContext Wrap(T value) => wrapper.Wrap(value);
        public static ExecutionContext WrapStatic() => wrapper.WrapStatic();


        public static T SafeUnwrap(ExecutionContext context) => (T)wrapper.SafeUnwrap(context);
        public static void SafeUnwrapStatic(ExecutionContext context) => wrapper.SafeUnwrapStatic(context);
    }

    /// <summary>
    /// Это свойство будет пропущено при разворачивании
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class WrapperSkipAttribute : Attribute
    {
        public WrapperSkipAttribute() { }
    }

    /// <summary>
    /// В это свойство типа string будет вписано имя контекста
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class WrapperContextNameAttribute : Attribute
    {
        public WrapperContextNameAttribute() { }
    }
}
