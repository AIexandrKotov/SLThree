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
        public static int count { get; set; } = 20;
        public static int max_output { get; set; } = 2500;
        public static object get_output(object value) => Program.GetOutput(value);
        public static void out_locals(ExecutionContext.ContextWrap context) => Program.OutLocals(context.pred, -2);
        public static void out_locals_typed(ExecutionContext.ContextWrap context) => Program.OutLocals(context.pred, -2, true);
        public static void new_context(ExecutionContext.ContextWrap context) => Program.REPLContext = context.pred;
        public static void command(string str) => Program.REPLCommand(">" + str);
        public static ExecutionContext.ContextWrap run_file(string file) 
            => new ExecutionContext.ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), Encoding.UTF8));
        public static ExecutionContext.ContextWrap run_file(string file, Encoding encoding) 
            => new ExecutionContext.ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), encoding));
        public static ExecutionContext.ContextWrap run_file_in(string file, ExecutionContext.ContextWrap context) 
            => new ExecutionContext.ContextWrap(Program.InvokeFile(file, context.pred, Encoding.UTF8));
        public static ExecutionContext.ContextWrap run_file_in(string file, Encoding encoding, ExecutionContext.ContextWrap context) 
            => new ExecutionContext.ContextWrap(Program.InvokeFile(file, context.pred, encoding));
    }
#pragma warning restore IDE1006 // Стили именования
}
