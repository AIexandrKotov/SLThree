using SLThree.Extensions;
using SLThree.Native;
using SLThree.sys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SLThree
{
    public static class Wrapper
    {
        #region Type Setting
        internal static Type generic_list = typeof(List<int>).GetGenericTypeDefinition();
        internal static Type generic_dict = typeof(Dictionary<string, int>).GetGenericTypeDefinition();
        internal static Type type_ituple = typeof(ITuple);
        public static bool HasRecast(Type type)
        {
            if (type.IsArray) return true;
            if (type.IsGenericType)
            {
                if (type == generic_list) return true;
                if (type == generic_dict) return true;
            }
            var interfaces = type.GetInterfaces();
            if (interfaces.Contains(type_ituple)) return true;
            return false;
        }
        public static object[] TupleToArray(ITuple tuple)
        {
            var ret = new object[tuple.Length];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = tuple[i];
            return ret;
        }
        public static object UnwrapCast(Type type_to_cast, object o)
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
            return o.CastToType(type_to_cast);
        }
        public static object WrapCast(object o)
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
            else if (type.IsTuple())
            {
                return CreatorTuple.Create(linq.entuple((ITuple)o).ToArray());
            }
            return o;
        }
        #endregion

        /// <summary>
        /// Пропускает этот элемент
        /// </summary>
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
        public sealed class IgnoreAttribute : Attribute
        {

        }

        /// <summary>
        /// Выделяет элемент для хранения имени
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class NameAttribute : Attribute
        {

        }

        /// <summary>
        /// Выключает неявные приведения при разворачивании для данного элемента
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class StrongUnwrapAttribute : Attribute
        {

        }

        /// <summary>
        /// Выключает неявные приведения при сворачивании для данного элемента
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class StrongWrapAttribute : Attribute
        {

        }

        /// <summary>
        /// Заставляет элемент проводить разворачивание и сворачивание словно контекст
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class ContextAttribute : Attribute
        {

        }

        /// <summary>
        /// Позволяет свернуть этот readonly-элемент в контекст
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class WrapReadonlyAttribute : Attribute
        {

        }

        /// <summary>
        /// Задаёт конструктор, который будет использован при разворачивании
        /// </summary>
        [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
        public sealed class ConstructorAttribute : Attribute
        {

        }

        /// <summary>
        /// Выделяет метод, возвращающий аргументы для конструктора
        /// сигнатуры static Func&lt;ExecutionContext, object[]&gt;
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        public sealed class ConstructorArgsAttribute : Attribute
        {

        }

        private class WrappingMemberInfo
        {
            public MemberInfo MemberInfo;
            public bool IsStrongWrap;
            public bool IsStrongUnwrap;
            public bool IsWrapReadonly;
            public bool IsContext;
            public bool IsField;
            public int id = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>(элементы, элемент-имя, конструктор, метод для аргументов конструктора)</returns>
        private static (List<WrappingMemberInfo>, WrappingMemberInfo, ConstructorInfo, MethodInfo) Collect(Type type)
        {
            var ret = new List<WrappingMemberInfo>();
            var name = default(WrappingMemberInfo);
            var constructor = default(ConstructorInfo);
            var constructorargs = default(MethodInfo);
            var id = 0;

            void Check(MemberInfo member)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var field = (FieldInfo)member;
                            if (field.IsInitOnly && member.GetCustomAttribute<WrapReadonlyAttribute>() == null) return;
                            var m = new WrappingMemberInfo
                            {
                                MemberInfo = member,
                                IsWrapReadonly = field.IsInitOnly,
                                IsContext = member.GetCustomAttribute<ContextAttribute>() != null,
                                IsStrongUnwrap = member.GetCustomAttribute<StrongUnwrapAttribute>() != null,
                                IsStrongWrap = member.GetCustomAttribute<StrongWrapAttribute>() != null,
                                IsField = true,
                                id = id++,
                            };
                            if (member.GetCustomAttribute<NameAttribute>() == null) ret.Add(m);
                            else
                            {
                                if (field.IsInitOnly) throw new NotSupportedException($"Wrapper.Name cannot be readonly [{field}]");
                                if (name != null) throw new NotSupportedException($"Wrapper.Name has already been defined [{field}]");
                                name = m;
                                id--;
                            }
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var property = (PropertyInfo)member;
                            if (!property.CanRead) return;
                            if (!property.CanWrite && member.GetCustomAttribute<WrapReadonlyAttribute>() == null) return;
                            if (property.GetIndexParameters().Length != 0) return;
                            var m = new WrappingMemberInfo
                            {
                                MemberInfo = member,
                                IsWrapReadonly = !property.CanWrite,
                                IsContext = member.GetCustomAttribute<ContextAttribute>() != null,
                                IsStrongUnwrap = member.GetCustomAttribute<StrongUnwrapAttribute>() != null,
                                IsStrongWrap = member.GetCustomAttribute<StrongWrapAttribute>() != null,
                                id = id++,
                            };
                            if (member.GetCustomAttribute<NameAttribute>() == null) ret.Add(m);
                            else
                            {
                                if (!property.CanWrite) throw new NotSupportedException($"Wrapper.Name cannot be readonly [{property}]");
                                if (name != null) throw new NotSupportedException($"Wrapper.Name has already been defined [{property}]");
                                name = m;
                                id--;
                            }
                        }
                        break;
                    case MemberTypes.Constructor:
                        {
                            var ctor = (ConstructorInfo)member;
                            if (ctor.GetParameters().Length == 0)
                            {
                                constructor = ctor;
                                return;
                            }
                            if (member.GetCustomAttribute(typeof(ConstructorAttribute)) == null) return;
                            if (constructor != null && constructor.GetParameters().Length != 0) throw new NotSupportedException($"Explicit constructor has already been defined [{constructor}]");
                            constructor = ctor;
                        }
                        break;
                }
            }

            void CheckCtorArgs(MemberInfo member)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        {
                            if (member.GetCustomAttribute<ConstructorArgsAttribute>() == null) return;
                            var method = (MethodInfo)member;
                            var pars = method.GetParameters();
                            if (method.ReturnType != typeof(object[]) || !(pars.Length == 1 && pars[0].ParameterType == ContextType)) throw new NotSupportedException($"Wrong signature... [{method}]");
                            constructorargs = method;
                        }
                        break;
                }
            }

            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (member.GetCustomAttribute<IgnoreAttribute>() != null) continue;
                Check(member);
            }

            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                if (member.GetCustomAttribute<IgnoreAttribute>() != null) continue;
                CheckCtorArgs(member);
            }

            if (constructor != null && constructor.GetParameters().Length != 0 && constructorargs == null)
            {
                throw new NotSupportedException($"{type.Name} must have Wrapper.ConstructorArgs method because it has an explicit constructor");
            }

            return (ret, name, constructor, constructorargs);
        }
        private static (List<WrappingMemberInfo>, WrappingMemberInfo) CollectStatic(Type type)
        {
            var ret = new List<WrappingMemberInfo>();
            var name = default(WrappingMemberInfo);
            var id = 0;

            void Check(MemberInfo member)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var field = (FieldInfo)member;
                            if (field.IsInitOnly && member.GetCustomAttribute<WrapReadonlyAttribute>() == null) return;
                            var m = new WrappingMemberInfo
                            {
                                MemberInfo = member,
                                IsWrapReadonly = type.IsEnum ? true : field.IsInitOnly,
                                IsContext = member.GetCustomAttribute<ContextAttribute>() != null,
                                IsStrongUnwrap = member.GetCustomAttribute<StrongUnwrapAttribute>() != null,
                                IsStrongWrap = member.GetCustomAttribute<StrongWrapAttribute>() != null,
                                IsField = true,
                                id = id++,
                            };
                            if (member.GetCustomAttribute<NameAttribute>() == null) ret.Add(m);
                            else
                            {
                                if (field.IsInitOnly) throw new NotSupportedException($"StaticWrapper.Name cannot be readonly [{field}]");
                                if (name != null) throw new NotSupportedException($"StaticWrapper.Name has already been defined [{field}]");
                                name = m;
                                id--;
                            }
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var property = (PropertyInfo)member;
                            if (!property.CanRead) return;
                            if (!property.CanWrite && member.GetCustomAttribute<WrapReadonlyAttribute>() == null) return;
                            var m = new WrappingMemberInfo
                            {
                                MemberInfo = member,
                                IsWrapReadonly = !property.CanWrite,
                                IsContext = member.GetCustomAttribute<ContextAttribute>() != null,
                                IsStrongUnwrap = member.GetCustomAttribute<StrongUnwrapAttribute>() != null,
                                IsStrongWrap = member.GetCustomAttribute<StrongWrapAttribute>() != null,
                                id = id++,
                            };
                            if (member.GetCustomAttribute<NameAttribute>() == null) ret.Add(m);
                            else
                            {
                                if (!property.CanWrite) throw new NotSupportedException($"StaticWrapper.Name cannot be readonly [{property}]");
                                if (name != null) throw new NotSupportedException($"StaticWrapper.Name has already been defined [{property}]");
                                name = m;
                                id--;
                            }
                        }
                        break;
                }
            }

            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                if (member.GetCustomAttribute<IgnoreAttribute>() != null) continue;
                Check(member);
            }

            return (ret, name);
        }

        private static Type ContextType = typeof(ExecutionContext);
        private static Type LocalsType = typeof(LocalVariablesContainer);
        private static Type Void = typeof(void);

        internal static MethodInfo GetPropertyGetter(PropertyInfo propertyInfo) => propertyInfo.GetAccessors(false).FirstOrDefault(x => x.ReturnType != Void);
        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo) => propertyInfo.GetAccessors(false).FirstOrDefault(x => x.ReturnType == Void);
        private static Type GetElementType(WrappingMemberInfo elem) => elem.IsField ? ((FieldInfo)elem.MemberInfo).FieldType : ((PropertyInfo)elem.MemberInfo).PropertyType;

        internal static (Delegate, Delegate, Delegate, Delegate, Delegate, Dictionary<string, int>) InitStaticWrapper(Type type)
        {
            var (elems, nameelem) = CollectStatic(type);

            void LoadElement(WrappingMemberInfo elem, ILGenerator il)
            {
                if (elem.IsField)
                {
                    NETGenerator.EmitLoadStaticFieldOrConst((FieldInfo)elem.MemberInfo, il);
                }
                else il.Emit(OpCodes.Callvirt, GetPropertyGetter((PropertyInfo)elem.MemberInfo));
            }

            void SetElement(WrappingMemberInfo elem, ILGenerator il)
            {
                if (elem.IsField) il.Emit(OpCodes.Stsfld, (FieldInfo)elem.MemberInfo);
                else il.Emit(OpCodes.Callvirt, GetPropertySetter((PropertyInfo)elem.MemberInfo));
            }

            var quickwrapper = default(Delegate);
            {
                var wrapper_dm = new DynamicMethod("QuickWrapper", Void, new Type[2] { ContextType, LocalsType });
                var wrapper_il = wrapper_dm.GetILGenerator();

                if (elems.Count > 0)
                {
                    wrapper_il.Emit(OpCodes.Ldarg_1);
                    wrapper_il.Emit(OpCodes.Ldfld, LocalsType.GetField("Variables"));
                }
                for (var i = 0; i < elems.Count; i++)
                {
                    if (i < elems.Count - 1) wrapper_il.Emit(OpCodes.Dup);
                    wrapper_il.Emit(OpCodes.Ldc_I4, elems[i].id);
                    LoadElement(elems[i], wrapper_il);

                    var elem_type = GetElementType(elems[i]);
                    if (elems[i].IsContext)
                    {
                        wrapper_il.Emit(OpCodes.Ldarg_1);
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("WrapUnder", BindingFlags.Public | BindingFlags.Static));
                    }
                    else
                    {
                        if (elem_type.IsValueType)
                            wrapper_il.Emit(OpCodes.Box, elem_type);
                    }
                    if (!elems[i].IsStrongWrap && HasRecast(elem_type))
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("WrapCast", BindingFlags.Public | BindingFlags.Static));

                    wrapper_il.Emit(OpCodes.Stelem_Ref);
                }

                wrapper_il.Emit(OpCodes.Ret);
                quickwrapper = wrapper_dm.CreateDelegate(typeof(Action<,>).MakeGenericType(new Type[2] { ContextType, LocalsType }));
            }

            var safewrapper = default(Delegate);
            {
                var wrapper_dm = new DynamicMethod("SafeWrapper", Void, new Type[] { ContextType, LocalsType });
                var wrapper_il = wrapper_dm.GetILGenerator();

                for (var i = 0; i < elems.Count; i++)
                {
                    wrapper_il.Emit(OpCodes.Ldarg_1);
                    wrapper_il.Emit(OpCodes.Ldstr, elems[i].MemberInfo.Name);
                    LoadElement(elems[i], wrapper_il);

                    var elem_type = GetElementType(elems[i]);
                    if (elems[i].IsContext)
                    {
                        wrapper_il.Emit(OpCodes.Ldarg_0);
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("WrapUnder", BindingFlags.Public | BindingFlags.Static));
                    }
                    else
                    {
                        if (elem_type.IsValueType)
                            wrapper_il.Emit(OpCodes.Box, elem_type);

                        if (!elems[i].IsStrongWrap && HasRecast(elem_type))
                            wrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("WrapCast", BindingFlags.Public | BindingFlags.Static));
                    }

                    wrapper_il.Emit(OpCodes.Callvirt, typeof(LocalVariablesContainer).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) }));
                    wrapper_il.Emit(OpCodes.Pop);
                }

                wrapper_il.Emit(OpCodes.Ret);
                safewrapper = wrapper_dm.CreateDelegate(typeof(Action<,>).MakeGenericType(new Type[] { ContextType, LocalsType }));
            }

            var unwrapper = default(Delegate);
            {
                var unwrapper_dm = new DynamicMethod("Unwrapper", Void, new Type[] { LocalsType });
                var unwrapper_il = unwrapper_dm.GetILGenerator();
                unwrapper_il.DeclareLocal(typeof(int));
                unwrapper_il.DeclareLocal(typeof(object[]));
                unwrapper_il.DeclareLocal(typeof(ContextWrap));

                unwrapper_il.Emit(OpCodes.Ldc_I4_0);
                unwrapper_il.Emit(OpCodes.Stloc_0);
                unwrapper_il.Emit(OpCodes.Ldarg_0);
                unwrapper_il.Emit(OpCodes.Ldfld, typeof(LocalVariablesContainer).GetField("NamedIdentificators"));
                unwrapper_il.Emit(OpCodes.Ldarg_0);
                unwrapper_il.Emit(OpCodes.Ldfld, typeof(LocalVariablesContainer).GetField("Variables"));
                unwrapper_il.Emit(OpCodes.Stloc_1);


                var label = default(Label);

                for (var i = 0; i < elems.Count; i++)
                {
                    if (elems[i].IsWrapReadonly) continue;
                    if (i < elems.Count - 1) unwrapper_il.Emit(OpCodes.Dup);

                    unwrapper_il.Emit(OpCodes.Ldstr, elems[i].MemberInfo.Name);
                    unwrapper_il.Emit(OpCodes.Ldloca_S, 0);
                    unwrapper_il.Emit(OpCodes.Callvirt, typeof(Dictionary<string, int>).GetMethod("TryGetValue"));
                    unwrapper_il.Emit(OpCodes.Brfalse_S, label = unwrapper_il.DefineLabel());


                    var elem_type = GetElementType(elems[i]);
                    if (elems[i].IsContext)
                    {
                        unwrapper_il.Emit(OpCodes.Ldloc_1);
                        unwrapper_il.Emit(OpCodes.Ldloc_0);
                        unwrapper_il.Emit(OpCodes.Ldelem_Ref);
                        unwrapper_il.Emit(OpCodes.Isinst, typeof(ContextWrap));
                        unwrapper_il.Emit(OpCodes.Stloc_2);
                        unwrapper_il.Emit(OpCodes.Ldloc_2);
                        unwrapper_il.Emit(OpCodes.Brfalse_S, label);

                        unwrapper_il.Emit(OpCodes.Ldloc_2);
                        //unwrapper_il.Emit(OpCodes.Ldfld, typeof(ContextWrap).GetField("Context"));
                        unwrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("UnwrapUnder", BindingFlags.Public | BindingFlags.Static));
                        SetElement(elems[i], unwrapper_il);
                    }
                    else
                    {
                        unwrapper_il.Emit(OpCodes.Ldloc_1);
                        unwrapper_il.Emit(OpCodes.Ldloc_0);
                        unwrapper_il.Emit(OpCodes.Ldelem_Ref);

                        if (elem_type.IsValueType) unwrapper_il.Emit(OpCodes.Unbox_Any, elem_type);
                        else unwrapper_il.Emit(OpCodes.Castclass, elem_type);

                        if (!elems[i].IsStrongUnwrap && HasRecast(elem_type))
                            unwrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("UnwrapCast", BindingFlags.Public | BindingFlags.Static));

                        SetElement(elems[i], unwrapper_il);
                    }
                    unwrapper_il.MarkLabel(label);
                }

                unwrapper_il.Emit(OpCodes.Ret);
                unwrapper = unwrapper_dm.CreateDelegate(typeof(Action<>).MakeGenericType(new Type[] { LocalsType }));
            }

            var wrapname = default(Delegate);
            {
                var wrapname_dm = new DynamicMethod("WrapName", Void, new Type[] { ContextType });
                var wrapname_il = wrapname_dm.GetILGenerator();
                if (nameelem != null)
                {
                    if (GetElementType(nameelem) != typeof(string)) throw new NotSupportedException($"Name must be string [{nameelem}]");

                    wrapname_il.Emit(OpCodes.Ldarg_1);
                    LoadElement(nameelem, wrapname_il);

                    wrapname_il.Emit(OpCodes.Stfld, ContextType.GetField("Name", BindingFlags.Public | BindingFlags.Instance));
                }
                wrapname_il.Emit(OpCodes.Ret);
                wrapname = wrapname_dm.CreateDelegate(typeof(Action<>).MakeGenericType(new Type[] { ContextType }));
            }

            var unwrapname = default(Delegate);
            {
                var wrapname_dm = new DynamicMethod("UnwrapName", Void, new Type[] { ContextType });
                var wrapname_il = wrapname_dm.GetILGenerator();
                if (nameelem != null)
                {
                    if (GetElementType(nameelem) != typeof(string)) throw new NotSupportedException($"Name must be string [{nameelem}]");

                    wrapname_il.Emit(OpCodes.Ldarg_0);
                    wrapname_il.Emit(OpCodes.Ldfld, ContextType.GetField("Name", BindingFlags.Public | BindingFlags.Instance));
                    SetElement(nameelem, wrapname_il);
                }
                wrapname_il.Emit(OpCodes.Ret);
                unwrapname = wrapname_dm.CreateDelegate(typeof(Action<>).MakeGenericType(new Type[] { ContextType }));
            }

            return (quickwrapper, safewrapper, unwrapper, wrapname, unwrapname, elems.ToDictionary(x => x.MemberInfo.Name, x => x.id));
        }
        internal static (Delegate, Delegate, Delegate, Delegate, Delegate, Delegate, Dictionary<string, int>) InitWrapper(Type type)
        {
            if (type.IsAbstract && type.IsSealed) throw new NotSupportedException("Non-static Wrapper<T> not supported static classes");

            var (elems, nameelem, explicit_constructor, explicit_constructor_args) = Collect(type);

            void LoadElement(WrappingMemberInfo elem, ILGenerator il)
            {
                if (elem.IsField) il.Emit(OpCodes.Ldfld, (FieldInfo)elem.MemberInfo);
                else il.Emit(OpCodes.Callvirt, GetPropertyGetter((PropertyInfo)elem.MemberInfo));
            }

            void SetElement(WrappingMemberInfo elem, ILGenerator il)
            {
                if (elem.IsField) il.Emit(OpCodes.Stfld, (FieldInfo)elem.MemberInfo);
                else il.Emit(OpCodes.Callvirt, GetPropertySetter((PropertyInfo)elem.MemberInfo));
            }

            var quickwrapper = default(Delegate);
            {
                var wrapper_dm = new DynamicMethod("QuickWrapper", Void, new Type[] { type, ContextType, LocalsType });
                var wrapper_il = wrapper_dm.GetILGenerator();

                if (elems.Count > 0)
                {
                    wrapper_il.Emit(OpCodes.Ldarg_2);
                    wrapper_il.Emit(OpCodes.Ldfld, LocalsType.GetField("Variables", BindingFlags.Public | BindingFlags.Instance));
                }

                for (var i = 0; i < elems.Count; i++)
                {
                    if (i < elems.Count - 1) wrapper_il.Emit(OpCodes.Dup);
                    wrapper_il.Emit(OpCodes.Ldc_I4, elems[i].id);

                    wrapper_il.Emit(OpCodes.Ldarg_0);
                    LoadElement(elems[i], wrapper_il);

                    var elem_type = GetElementType(elems[i]);
                    if (elems[i].IsContext)
                    {
                        wrapper_il.Emit(OpCodes.Ldarg_1);
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("WrapUnder", BindingFlags.Public | BindingFlags.Static));
                    }
                    else
                    {
                        if (elem_type.IsValueType)
                            wrapper_il.Emit(OpCodes.Box, elem_type);
                    }
                    if (!elems[i].IsStrongWrap && HasRecast(elem_type))
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("WrapCast", BindingFlags.Public | BindingFlags.Static));
                        
                    wrapper_il.Emit(OpCodes.Stelem_Ref);
                }

                wrapper_il.Emit(OpCodes.Ret);
                quickwrapper = wrapper_dm.CreateDelegate(typeof(Action<,,>).MakeGenericType(new Type[] { type, ContextType, LocalsType }));
            }

            var safewrapper = default(Delegate);
            {
                var wrapper_dm = new DynamicMethod("SafeWrapper", Void, new Type[] { type, ContextType, LocalsType });
                var wrapper_il = wrapper_dm.GetILGenerator();

                for (var i = 0; i < elems.Count; i++)
                {
                    wrapper_il.Emit(OpCodes.Ldarg_2);
                    wrapper_il.Emit(OpCodes.Ldstr, elems[i].MemberInfo.Name);
                    wrapper_il.Emit(OpCodes.Ldarg_0);
                    LoadElement(elems[i], wrapper_il);

                    var elem_type = GetElementType(elems[i]);
                    if (elems[i].IsContext)
                    {
                        wrapper_il.Emit(OpCodes.Ldarg_1);
                        wrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("WrapUnder", BindingFlags.Public | BindingFlags.Static));
                    }
                    else
                    {
                        if (elem_type.IsValueType)
                            wrapper_il.Emit(OpCodes.Box, elem_type);

                        if (!elems[i].IsStrongWrap && HasRecast(elem_type))
                            wrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("WrapCast", BindingFlags.Public | BindingFlags.Static));
                    }

                    wrapper_il.Emit(OpCodes.Callvirt, typeof(LocalVariablesContainer).GetMethod("SetValue", new Type[] {typeof(string), typeof(object)}));
                    wrapper_il.Emit(OpCodes.Pop);
                }

                wrapper_il.Emit(OpCodes.Ret);
                safewrapper = wrapper_dm.CreateDelegate(typeof(Action<,,>).MakeGenericType(new Type[] { type, ContextType, LocalsType }));
            }

            var unwrapper = default(Delegate);
            {
                var unwrapper_dm = new DynamicMethod("Unwrapper", Void, new Type[] { LocalsType, type });
                var unwrapper_il = unwrapper_dm.GetILGenerator();

                if (elems.Count > 0)
                {
                    unwrapper_il.DeclareLocal(typeof(int));
                    unwrapper_il.DeclareLocal(typeof(object[]));
                    unwrapper_il.DeclareLocal(typeof(ContextWrap));

                    unwrapper_il.Emit(OpCodes.Ldc_I4_0);
                    unwrapper_il.Emit(OpCodes.Stloc_0);
                    unwrapper_il.Emit(OpCodes.Ldarg_0);
                    unwrapper_il.Emit(OpCodes.Ldfld, typeof(LocalVariablesContainer).GetField("NamedIdentificators"));
                    unwrapper_il.Emit(OpCodes.Ldarg_0);
                    unwrapper_il.Emit(OpCodes.Ldfld, typeof(LocalVariablesContainer).GetField("Variables"));
                    unwrapper_il.Emit(OpCodes.Stloc_1);


                    var label = default(Label);

                    for (var i = 0; i < elems.Count; i++)
                    {
                        if (elems[i].IsWrapReadonly) continue;
                        if (i < elems.Count - 1) unwrapper_il.Emit(OpCodes.Dup);

                        unwrapper_il.Emit(OpCodes.Ldstr, elems[i].MemberInfo.Name);
                        unwrapper_il.Emit(OpCodes.Ldloca_S, 0);
                        unwrapper_il.Emit(OpCodes.Callvirt, typeof(Dictionary<string, int>).GetMethod("TryGetValue"));
                        unwrapper_il.Emit(OpCodes.Brfalse_S, label = unwrapper_il.DefineLabel());


                        var elem_type = GetElementType(elems[i]);
                        if (elems[i].IsContext)
                        {
                            unwrapper_il.Emit(OpCodes.Ldloc_1);
                            unwrapper_il.Emit(OpCodes.Ldloc_0);
                            unwrapper_il.Emit(OpCodes.Ldelem_Ref);
                            unwrapper_il.Emit(OpCodes.Isinst, typeof(ContextWrap));
                            unwrapper_il.Emit(OpCodes.Stloc_2);
                            unwrapper_il.Emit(OpCodes.Ldloc_2);
                            unwrapper_il.Emit(OpCodes.Brfalse_S, label);

                            unwrapper_il.Emit(OpCodes.Ldarg_1);
                            unwrapper_il.Emit(OpCodes.Ldloc_2);
                            //unwrapper_il.Emit(OpCodes.Ldfld, typeof(ContextWrap).GetField("Context"));
                            unwrapper_il.Emit(OpCodes.Call, typeof(Wrapper<>).MakeGenericType(new Type[] { elem_type }).GetMethod("UnwrapUnder", BindingFlags.Public | BindingFlags.Static));
                            SetElement(elems[i], unwrapper_il);
                        }
                        else
                        {
                            unwrapper_il.Emit(OpCodes.Ldarg_1);
                            unwrapper_il.Emit(OpCodes.Ldloc_1);
                            unwrapper_il.Emit(OpCodes.Ldloc_0);
                            unwrapper_il.Emit(OpCodes.Ldelem_Ref);

                            if (elem_type.IsValueType) unwrapper_il.Emit(OpCodes.Unbox_Any, elem_type);
                            else unwrapper_il.Emit(OpCodes.Castclass, elem_type);

                            if (!elems[i].IsStrongUnwrap && HasRecast(elem_type))
                                unwrapper_il.Emit(OpCodes.Call, typeof(Wrapper).GetMethod("UnwrapCast", BindingFlags.Public | BindingFlags.Static));

                            SetElement(elems[i], unwrapper_il);
                        }
                        unwrapper_il.MarkLabel(label);
                    }
                }

                unwrapper_il.Emit(OpCodes.Ret);
                unwrapper = unwrapper_dm.CreateDelegate(typeof(Action<,>).MakeGenericType(new Type[] { LocalsType, type }));
            }

            var wrapname = default(Delegate);
            {
                var wrapname_dm = new DynamicMethod("WrapName", Void, new Type[] { type, ContextType });
                var wrapname_il = wrapname_dm.GetILGenerator();
                if (nameelem != null)
                {
                    if (GetElementType(nameelem) != typeof(string)) throw new NotSupportedException($"Name must be string [{nameelem}]");

                    wrapname_il.Emit(OpCodes.Ldarg_1);
                    wrapname_il.Emit(OpCodes.Ldarg_0);
                    LoadElement(nameelem, wrapname_il);

                    wrapname_il.Emit(OpCodes.Stfld, ContextType.GetField("Name", BindingFlags.Public | BindingFlags.Instance));
                }
                wrapname_il.Emit(OpCodes.Ret);
                wrapname = wrapname_dm.CreateDelegate(typeof(Action<,>).MakeGenericType(new Type[] { type, ContextType }));
            }

            var unwrapname = default(Delegate);
            {
                var wrapname_dm = new DynamicMethod("UnwrapName", Void, new Type[] { ContextType, type });
                var wrapname_il = wrapname_dm.GetILGenerator();
                if (nameelem != null)
                {
                    if (GetElementType(nameelem) != typeof(string)) throw new NotSupportedException($"Name must be string [{nameelem}]");

                    wrapname_il.Emit(OpCodes.Ldarg_1);
                    wrapname_il.Emit(OpCodes.Ldarg_0);
                    wrapname_il.Emit(OpCodes.Ldfld, ContextType.GetField("Name", BindingFlags.Public | BindingFlags.Instance));
                    SetElement(nameelem, wrapname_il);
                }
                wrapname_il.Emit(OpCodes.Ret);
                unwrapname = wrapname_dm.CreateDelegate(typeof(Action<,>).MakeGenericType(new Type[] { ContextType, type }));
            }

            var initor = default(Delegate);
            if (explicit_constructor != null)
            {
                if (explicit_constructor.GetParameters().Length > 0)
                {
                    var initor_dm = new DynamicMethod("Initor", type, new Type[1] { ContextType }, true);
                    var initor_il = initor_dm.GetILGenerator();
                    initor_il.Emit(OpCodes.Ldarg_0);
                    initor_il.Emit(OpCodes.Call, explicit_constructor_args);
                    initor_il.DeclareLocal(typeof(object[]));
                    initor_il.Emit(OpCodes.Stloc_0);
                    var i = 0;
                    foreach (var p in explicit_constructor.GetParameters())
                    {
                        initor_il.Emit(OpCodes.Ldloc_0);
                        initor_il.Emit(OpCodes.Ldc_I4, i++);
                        initor_il.Emit(OpCodes.Ldelem_Ref);
                        if (p.ParameterType.IsValueType) initor_il.Emit(OpCodes.Unbox_Any, p.ParameterType);
                        else initor_il.Emit(OpCodes.Castclass, p.ParameterType);
                    }
                    initor_il.Emit(OpCodes.Newobj, explicit_constructor);
                    initor_il.Emit(OpCodes.Ret);
                    initor = initor_dm.CreateDelegate(typeof(Func<,>).MakeGenericType(new Type[] { ContextType, type }));
                }
                else
                {
                    var initor_dm = new DynamicMethod("Initor", type, new Type[1] { ContextType }, true);
                    var initor_il = initor_dm.GetILGenerator();
                    initor_il.Emit(OpCodes.Newobj, explicit_constructor);
                    initor_il.Emit(OpCodes.Ret);
                    initor = initor_dm.CreateDelegate(typeof(Func<,>).MakeGenericType(new Type[] { ContextType, type }));
                }
            }

            return (quickwrapper, safewrapper, unwrapper, wrapname, unwrapname, initor, elems.ToDictionary(x => x.MemberInfo.Name, x => x.id));
        }

        #region Extensions
        public static ExecutionContext Wrap<T>(this T obj) => Wrapper<T>.Wrap(obj);
        public static T Unwrap<T>(this ExecutionContext context) => Wrapper<T>.Unwrap(context);
        public static void WrapIn<T>(this T obj, ExecutionContext context) => Wrapper<T>.WrapIn(obj, context);
        public static void UnwrapIn<T>(this ExecutionContext context, T obj) => Wrapper<T>.UnwrapIn(context, obj);
        public static ExecutionContext StaticWrap<T>() => StaticWrapper<T>.Wrap();
        public static void StaticUnwrap<T>(this ExecutionContext context) => StaticWrapper<T>.Unwrap(context);
        public static void StaticWrapIn<T>(this ExecutionContext context) => StaticWrapper<T>.WrapIn(context);
        public static void StaticUnwrapIn<T>(this ExecutionContext context) => StaticWrapper<T>.UnwrapIn(context);

        public static readonly NonGenericWrapper NonGeneric = new NonGenericWrapper(typeof(object));
        public static readonly NonGenericStaticWrapper NonGenericStatic = new NonGenericStaticWrapper(typeof(object));
        public static ExecutionContext StaticWrap(this Type T) => NonGenericStatic[T].Wrap();
        public static void StaticUnwrap(this ExecutionContext context, Type T) => NonGenericStatic[T].Unwrap(context);
        public static void StaticUnwrap(this Type T, ExecutionContext context) => NonGenericStatic[T].Unwrap(context);
        public static void StaticWrapIn(this Type T, ExecutionContext context) => NonGenericStatic[T].WrapIn(context);
        public static void StaticWrapIn(this ExecutionContext context, Type T) => NonGenericStatic[T].WrapIn(context);
        public static void StaticUnwrapIn(this ExecutionContext context, Type T) => NonGenericStatic[T].UnwrapIn(context);
        public static void StaticUnwrapIn(this Type T, ExecutionContext context) => NonGenericStatic[T].UnwrapIn(context);
        public static ExecutionContext Wrap(this object obj, Type T) => NonGeneric[T].Wrap(obj);
        public static ExecutionContext InstanceWrap(this Type T, object obj) => NonGeneric[T].Wrap(obj);
        public static object Unwrap(this ExecutionContext context, Type T) => NonGeneric[T].Unwrap(context);
        public static object InstanceUnwrap(this Type T, ExecutionContext context) => NonGeneric[T].Unwrap(context);
        public static void WrapIn(this object obj, Type T, ExecutionContext context) => NonGeneric[T].WrapIn(obj, context);
        public static void InstanceWrapIn(this Type T, object obj, ExecutionContext context) => NonGeneric[T].WrapIn(obj, context);
        public static void UnwrapIn(this ExecutionContext context, Type T, object obj) => NonGeneric[T].UnwrapIn(context, obj);
        public static void InstanceUnwrapIn(this Type T, ExecutionContext context, object obj) => NonGeneric[T].UnwrapIn(context, obj);
        public static bool HasInitor(this Type T) => NonGeneric[T].HasInitior;
        public sealed class NonGenericStaticWrapper
        {
            public readonly MethodInfo mWrap;
            public readonly MethodInfo mUnwrap;
            public readonly MethodInfo mWrapIn;
            public readonly MethodInfo mUnwrapIn;

            internal NonGenericStaticWrapper(Type T)
            {
                var type = typeof(StaticWrapper<>).MakeGenericType(T);
                mWrap = type.GetMethod("Wrap");
                mUnwrap = type.GetMethod("Unwrap");
                mWrapIn = type.GetMethod("WrapIn");
                mUnwrapIn = type.GetMethod("UnwrapIn");
                nongeneric_wrappers[T] = this;
            }

            private static readonly Dictionary<Type, NonGenericStaticWrapper> nongeneric_wrappers = new Dictionary<Type, NonGenericStaticWrapper>();
            public NonGenericStaticWrapper this[Type type]
            {
                get
                {
                    if (nongeneric_wrappers.TryGetValue(type, out var ret)) return ret;
                    return new NonGenericStaticWrapper(type);
                }
            }

            public ExecutionContext Wrap() => (ExecutionContext)mWrap.Invoke(null, Array.Empty<object>());
            public void Unwrap(ExecutionContext context) => mUnwrap.Invoke(null, new object[] { context });
            public void WrapIn(ExecutionContext context) => mWrapIn.Invoke(null, new object[] { context });
            public void UnwrapIn(ExecutionContext context) => mUnwrapIn.Invoke(null, new object[] { context });
        }
        public sealed class NonGenericWrapper
        {
            public readonly MethodInfo mWrap;
            public readonly MethodInfo mUnwrap;
            public readonly MethodInfo mWrapIn;
            public readonly MethodInfo mUnwrapIn;
            public readonly bool HasInitior;

            internal NonGenericWrapper(Type T)
            {
                var type = typeof(Wrapper<>).MakeGenericType(T);
                mWrap = type.GetMethod("Wrap");
                mUnwrap = type.GetMethod("Unwrap");
                mWrapIn = type.GetMethod("WrapIn");
                mUnwrapIn = type.GetMethod("UnwrapIn");
                HasInitior = (bool)type.GetField("HasInitor").GetValue(null);
                nongeneric_wrappers[T] = this;
            }

            private static readonly Dictionary<Type, NonGenericWrapper> nongeneric_wrappers = new Dictionary<Type, NonGenericWrapper>();
            public NonGenericWrapper this[Type type]
            {
                get
                {
                    if (nongeneric_wrappers.TryGetValue(type, out var ret)) return ret;
                    return new NonGenericWrapper(type);
                }
            }

            public ExecutionContext Wrap(object obj) => (ExecutionContext)mWrap.Invoke(null, new object[] { obj });
            public object Unwrap(ExecutionContext context) => mUnwrap.Invoke(null, new object[] { context });
            public void WrapIn(object obj, ExecutionContext context) => mWrapIn.Invoke(null, new object[] { obj, context });
            public void UnwrapIn(ExecutionContext context, object obj) => mUnwrapIn.Invoke(null, new object[] { context, obj });
        }
        #endregion
    }

    public static class Wrapper<T>
    {
        public static readonly Func<ExecutionContext, T> Initor;
        public static readonly bool HasInitor;

        //Быстрое оборачивание заполняет переменные по отступу
        public static readonly Action<T, ExecutionContext, LocalVariablesContainer> QuickWrapper;
        //Безопасное оборачивание заполняет переменные по имени
        public static readonly Action<T, ExecutionContext, LocalVariablesContainer> SafeWrapper;
        public static readonly Action<LocalVariablesContainer, T> Unwrapper;

        public static readonly Action<T, ExecutionContext> WrapName;
        public static readonly Action<ExecutionContext, T> UnwrapName;

        public static readonly Dictionary<string, int> ContextNames;
        public static readonly string[] Names;
        public static readonly int ContextSize;

        static Wrapper()
        {
            var d = Wrapper.InitWrapper(typeof(T));
            QuickWrapper = (Action<T, ExecutionContext, LocalVariablesContainer>)d.Item1;
            SafeWrapper = (Action<T, ExecutionContext, LocalVariablesContainer>)d.Item2;
            Unwrapper = (Action<LocalVariablesContainer, T>)d.Item3;
            WrapName = (Action<T, ExecutionContext>)d.Item4;
            UnwrapName = (Action<ExecutionContext, T>)d.Item5;
            Initor = (Func<ExecutionContext, T>)d.Item6;
            HasInitor = Initor != null;
            ContextNames = d.Item7;
            Names = ContextNames.Keys.ToArray();
            ContextSize = ContextNames.Count;
        }

        public static ExecutionContext Wrap(T obj)
        {
            var lc = new LocalVariablesContainer(ContextSize, new Dictionary<string, int>(ContextNames));
            var ret = new ExecutionContext(false, false, lc);
            QuickWrapper(obj, ret, lc);
            WrapName(obj, ret);
            return ret;
        }
        public static ContextWrap WrapUnder(T obj, ExecutionContext context)
        {
            if (obj == null) return null;
            if (Wrapper.HasRecast(obj.GetType())) throw new NotSupportedException("This type can't be converted into context");
            var lc = new LocalVariablesContainer(ContextSize, new Dictionary<string, int>(ContextNames));
            var ret = new ExecutionContext(context, false, lc);
            QuickWrapper(obj, ret, lc);
            WrapName(obj, ret);
            return ret.wrap;
        }
        public static T Unwrap(ExecutionContext context)
        {
            if (HasInitor)
            {
                var ret = Initor(context);
                UnwrapName(context, ret);
                Unwrapper(context.LocalVariables, ret);
                return ret;
            }
            throw new NotSupportedException($"{typeof(T)} should have constructor");
        }
        public static T UnwrapUnder(ContextWrap context) => Unwrap(context.Context);
        public static void WrapIn(T obj, ExecutionContext context)
        {
            SafeWrapper(obj, context, context.LocalVariables);
            WrapName(obj, context);
        }
        public static void UnwrapIn(ExecutionContext context, T obj)
        {
            Unwrapper(context.LocalVariables, obj);
            UnwrapName(context, obj);
        }
    }
    public static class StaticWrapper<T>
    {
        public static readonly Action<ExecutionContext, LocalVariablesContainer> QuickWrapper;
        public static readonly Action<ExecutionContext, LocalVariablesContainer> SafeWrapper;
        public static readonly Action<LocalVariablesContainer> Unwrapper;

        public static readonly Action<ExecutionContext> WrapName;
        public static readonly Action<ExecutionContext> UnwrapName;

        public static readonly Dictionary<string, int> ContextNames;
        public static readonly string[] Names;
        public static readonly int ContextSize;

        static StaticWrapper()
        {
            var d = Wrapper.InitStaticWrapper(typeof(T));
            QuickWrapper = (Action<ExecutionContext, LocalVariablesContainer>)d.Item1;
            SafeWrapper = (Action<ExecutionContext, LocalVariablesContainer>)d.Item2;
            Unwrapper = (Action<LocalVariablesContainer>)d.Item3;
            WrapName = (Action<ExecutionContext>)d.Item4;
            UnwrapName = (Action<ExecutionContext>)d.Item5;
            ContextNames = d.Item6;
            Names = ContextNames.Keys.ToArray();
            ContextSize = ContextNames.Count;
        }

        public static ExecutionContext Wrap()
        {
            var lc = new LocalVariablesContainer(ContextSize, new Dictionary<string, int>(ContextNames));
            var ret = new ExecutionContext(false, false, lc);
            QuickWrapper(ret, lc);
            WrapName(ret);
            return ret;
        }
        public static void Unwrap(ExecutionContext context)
        {
            UnwrapName(context);
            Unwrapper(context.LocalVariables);
        }
        public static void WrapIn(ExecutionContext context)
        {
            SafeWrapper(context, context.LocalVariables);
            WrapName(context);
        }
        public static void UnwrapIn(ExecutionContext context)
        {
            Unwrapper(context.LocalVariables);
            UnwrapName(context);
        }
    }
}
