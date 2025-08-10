using System;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class di
    {
        public static IServiceProvider GetProvider() => ExecutionContext.global.Context.@private["%DI"] as IServiceProvider;

        public static void RegisterSLThreeServiceProvider(this IServiceProvider serviceProvider)
        {
            ExecutionContext.global.Context.@private["%DI%"] = serviceProvider;
        }

        public static object GetService<T>()
        {
            return GetProvider().GetService(typeof(T));
        }

        public static object GetService(Type type)
        {
            return GetProvider().GetService(type);
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
