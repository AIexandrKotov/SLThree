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
        public static T Unwrap<T>(this ExecutionContext context) where T : new() => Wrapper<T>.Unwrap(context);
    }
}
