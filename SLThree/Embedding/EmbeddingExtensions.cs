using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Embedding
{
    public static class EmbeddingExtensions
    {
        public static ExecutionContext RunScript(this string code)
        {
            return Parser.This.RunScript(code, null);
        }
        public static ExecutionContext RunScript(this string code, ExecutionContext context)
        {
            return Parser.This.RunScript(code, null, context);
        }
        public static ExecutionContext RunScript(this ExecutionContext context, string code)
        {
            return Parser.This.RunScript(code, null, context);
        }

        public static T Unwrap<T>(this ExecutionContext context) where T : new() => UnwrapperForInstances<T>.Unwrap(context);
        public static void UnwrapStatic<T>(this ExecutionContext context) => Wrapper<T>.UnwrapStatic(context);
        public static ExecutionContext Wrap<T>(this T obj) => Wrapper<T>.Wrap(obj);
        public static ExecutionContext WrapStatic<T>() => Wrapper<T>.WrapStatic();
        public static void UnwrapStaticClass(this ExecutionContext context, Type type) => UnwrapperForStaticClasses.Unwrap(type, context);
        public static void UnwrapStaticClass(this Type type, ExecutionContext context) => UnwrapperForStaticClasses.Unwrap(type, context);
        public static ExecutionContext WrapStaticClass(this Type type) => UnwrapperForStaticClasses.Wrap(type);

        public static T SafeUnwrap<T>(this ExecutionContext context) where T : new() => UnwrapperForInstances<T>.SafeUnwrap(context);
        public static void SafeUnwrapStatic<T>(this ExecutionContext context) => Wrapper<T>.SafeUnwrapStatic(context);
        public static void SafeUnwrapStaticClass(this ExecutionContext context, Type type) => UnwrapperForStaticClasses.SafeUnwrap(type, context);
        public static void SafeUnwrapStaticClass(this Type type, ExecutionContext context) => UnwrapperForStaticClasses.SafeUnwrap(type, context);
    }
}
