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
        public static ExecutionContext RunPreset(this string code, ExecutionContext preset)
        {
            return Parser.This.RunScript(code, null, null, preset);
        }
    }
}
