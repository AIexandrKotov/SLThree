using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static List<JIT.AbstractNameInfo> collect_vars(Method method, ExecutionContext context)
        {
            return JIT.NameCollector.Collect(method, context).Item1;
        }

        public static MethodInfo opt(Method method, ExecutionContext.ContextWrap context)
        {
            var rettype = method.ReturnType?.GetStaticValue();
            var ptypes = method.ParamTypes.ConvertAll(x => x.GetStaticValue());
            var dt = Module.DefineType($"type{index++}", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            Type t;
            var mb = default(MethodBuilder);
            try
            {
                mb = dt.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Static, rettype, ptypes);
                var ng = new JIT.NETGenerator(method, context.pred, mb, mb.GetILGenerator());
                ng.Visit(method);
            }
            catch
            {
                var il = mb.GetILGenerator();
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ret);
            }
            finally
            {
                t = dt.CreateType();
            }
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
