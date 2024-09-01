using SLThree;
using System.Text;

namespace slt.sys
{
#pragma warning disable IDE1006 // Стили именования
#pragma warning disable CS8981 // Имя типа содержит только строчные символы ASCII. Такие имена могут резервироваться для языка.
    public static class repl
#pragma warning restore CS8981 // Имя типа содержит только строчные символы ASCII. Такие имена могут резервироваться для языка.
    {
        public static int count { get; set; } = 20;
        public static int max_output { get; set; } = 2500;
        public static object get_output(object value) => Program.GetOutput(value);
        public static void out_locals(ContextWrap context) => Program.OutLocals(context.Context, -2);
        public static void out_locals_typed(ContextWrap context) => Program.OutLocals(context.Context, -2, true);
        public static void new_context(ContextWrap context) => Program.REPLContext = context.Context;
        public static void command(string str) => Program.REPLCommand(">" + str);
        public static ContextWrap run_file(string file)
            => new ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), Encoding.UTF8));
        public static ContextWrap run_file(string file, Encoding encoding)
            => new ContextWrap(Program.InvokeFile(file, Program.GetNewREPLContext(), encoding));
        public static ContextWrap run_file_in(string file, ContextWrap context)
            => new ContextWrap(Program.InvokeFile(file, context.Context, Encoding.UTF8));
        public static ContextWrap run_file_in(string file, Encoding encoding, ContextWrap context)
            => new ContextWrap(Program.InvokeFile(file, context.Context, encoding));
    }
#pragma warning restore IDE1006 // Стили именования
}
