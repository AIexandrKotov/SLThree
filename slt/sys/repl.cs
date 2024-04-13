using SLThree;
using System.Text;

namespace slt.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class repl
    {
        public static int count { get; set; } = 20;
        public static int max_output { get; set; } = 2500;
        public static object get_output(object value) => Program.GetOutput(value);
        public static void out_locals(ContextWrap context) => Program.OutLocals(context.pred, -2);
        public static void out_locals_typed(ContextWrap context) => Program.OutLocals(context.pred, -2, true);
        public static void new_context(ContextWrap context) => Program.REPLContext = context.pred;
        public static void command(string str) => Program.REPLCommand(">" + str);
        public static ContextWrap run_file(string file)
            => new ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), Encoding.UTF8));
        public static ContextWrap run_file(string file, Encoding encoding)
            => new ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), encoding));
        public static ContextWrap run_file_in(string file, ContextWrap context)
            => new ContextWrap(Program.InvokeFile(file, context.pred, Encoding.UTF8));
        public static ContextWrap run_file_in(string file, Encoding encoding, ContextWrap context)
            => new ContextWrap(Program.InvokeFile(file, context.pred, encoding));
    }
#pragma warning restore IDE1006 // Стили именования
}
