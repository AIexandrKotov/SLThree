using System;

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

        public static T Unwrap<T>(this ExecutionContext context) => Wrapper<T>.Unwrap(context);
        public static void UnwrapStatic<T>(this ExecutionContext context) => Wrapper<T>.UnwrapStatic(context);
        public static ExecutionContext Wrap<T>(this T obj) => Wrapper<T>.Wrap(obj);
        public static ExecutionContext WrapStatic<T>() => Wrapper<T>.WrapStatic();
        public static void UnwrapStaticClass(this ExecutionContext context, Type type) => NonGenericWrapper.GetWrapper(type).UnwrapStaticClass(context);
        public static void UnwrapStaticClass(this Type type, ExecutionContext context) => NonGenericWrapper.GetWrapper(type).UnwrapStaticClass(context);
        public static ExecutionContext WrapStaticClass(this Type type) => NonGenericWrapper.GetWrapper(type).WrapStaticClass();

        public static T SafeUnwrap<T>(this ExecutionContext context) => Wrapper<T>.SafeUnwrap(context);
        public static void SafeUnwrapStatic<T>(this ExecutionContext context) => Wrapper<T>.SafeUnwrapStatic(context);
        public static void SafeUnwrapStaticClass(this ExecutionContext context, Type type) => NonGenericWrapper.GetWrapper(type).SafeUnwrapStaticClass(context);
        public static void SafeUnwrapStaticClass(this Type type, ExecutionContext context) => NonGenericWrapper.GetWrapper(type).SafeUnwrapStaticClass(context);
    }
}
