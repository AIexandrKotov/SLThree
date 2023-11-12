using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLThree;
using SLThree.Embedding;

namespace slt.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class repl
    {
        public static object get_output(object value) => Program.GetOutput(value);
        public static void out_locals(ExecutionContext.ContextWrap context) => Program.OutLocals(context.pred, -2);
        public static void out_locals_typed(ExecutionContext.ContextWrap context) => Program.OutLocals(context.pred, -2, true);
        public static void new_context(ExecutionContext.ContextWrap context) => Program.REPLContext = context.pred;
    }
#pragma warning restore IDE1006 // Стили именования
}
