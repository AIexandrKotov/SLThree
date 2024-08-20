using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class jit
    {
        static AssemblyName Name = new AssemblyName("SLThree.JIT");
        static AssemblyBuilder Assembly;
        static ModuleBuilder Module;
        static int index;

        static jit()
        {
#if NETFRAMEWORK
            Assembly = AssemblyBuilder.DefineDynamicAssembly(Name, AssemblyBuilderAccess.RunAndSave);
            Module = Assembly.DefineDynamicModule("module", "module.exe", true);
#else
            Assembly = AssemblyBuilder.DefineDynamicAssembly(Name, AssemblyBuilderAccess.Run);
            Module = Assembly.DefineDynamicModule("module");
#endif
        }

        public static List<Native.AbstractNameInfo> collect_vars(Method method, ExecutionContext context)
        {
            return Native.NameCollector.Collect(method, context).Item1;
        }

        public static MethodInfo opt(Method method, ContextWrap context)
        {
            var rettype = method.ReturnType?.GetStaticValue();
            var ptypes = method.ParamTypes.ConvertAll(x => x.GetStaticValue());
            var dt = Module.DefineType($"type{index++}", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            Type t;
            var mb = default(MethodBuilder);
            var rethrow = default(Exception);
            try
            {
                mb = dt.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Static, rettype, ptypes);
                var ng = new Native.NETGenerator(method, context.Context, mb, mb.GetILGenerator());
                ng.Visit(method);
            }
            catch (Exception e)
            {
                var il = mb.GetILGenerator();
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ret);
                rethrow = e;
            }
            finally
            {
                t = dt.CreateType();
            }
            if (rethrow != null) throw rethrow;
            return t.GetMethod(method.Name);
        }

        public static void save()
        {
#if NETFRAMEWORK
            Assembly.Save("module.exe");
            Assembly = AssemblyBuilder.DefineDynamicAssembly(Name, AssemblyBuilderAccess.RunAndSave);
            Module = Assembly.DefineDynamicModule("module", "module.exe", true);
#else
            throw new Exception("Saving assemblies allowed only on .NET Framework");
#endif
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
