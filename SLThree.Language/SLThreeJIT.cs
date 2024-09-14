using System;
using System.Reflection;

namespace SLThree
{
    public static class SLThreeJIT
    {
        public enum MethodType
        {
            /// <summary>
            /// Constructed .NET Framework platform method
            /// </summary>
            Dotnet = -1,
            /// <summary>
            /// Basic abstract syntax version of Method
            /// </summary>
            AST = 0,
            /// <summary>
            /// Constructed .NET Framework platform method. Has ExecutionContext argument and AST-insertions
            /// </summary>
            Mixed = 1,
        }

        public static MethodInfo TryToJIT(Method method, ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
