using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{

    public class ExecutionContext
    {
        public interface IExecutable
        {
            object GetValue(ExecutionContext context);
        }

        public T GetCasted<T>(string name) => LocalVariables.GetValue(name).Item1.Cast<T>();

        /// <summary>
        /// Имя данного контекста
        /// </summary>
        public string Name = $"@{Convert.ToString(context_count++, 16).ToUpper().PadLeft(4, '0')}";
        private static long context_count = 0;

        /// <summary>
        /// Запрещает implicit в контексте
        /// </summary>
        public bool fimp = false;

        internal bool Returned;
        internal bool Broken;
        internal bool Continued;

        public object ReturnedValue;

        public List<Exception> Errors = new List<Exception>();

        public static ContextWrap global = new ContextWrap(new ExecutionContext(false) { fimp = false, Name = "global" });

        static ExecutionContext()
        {

        }

        public ExecutionContext(bool assign_to_global)
        {
            @this = new ContextWrap(this);
            wrap = new ContextWrap(this);
            if (assign_to_global) toplevel = global.pred;
        }

        public ExecutionContext(ExecutionContext context)
        {
            @this = new ContextWrap(this);
            wrap = new ContextWrap(this);
            if (context != null) toplevel = context;
        }

        public ExecutionContext() : this(true) { }

        public ContextWrap @this;

        public class ContextWrap : IEnumerable<object>
        {
            public ExecutionContext pred;

            public ContextWrap(ExecutionContext pred)
            {
                this.pred = pred;
            }

            public static Func<object, object> Decoration = o => o;

            public string ToDetailedString(int index, List<ContextWrap> outed_contexts)
            {
                var sb = new StringBuilder();
                outed_contexts.Add(this);

                sb.AppendLine($"context {pred.Name} {{");
                foreach (var x in pred.LocalVariables.GetAsDictionary())
                {
                    
                    sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}{x.Key} = ");
                    if (x.Value is ContextWrap wrap)
                    {
                        if (outed_contexts.Contains(wrap)) sb.AppendLine($"context {wrap.pred.Name}; //recursive");
                        else sb.AppendLine(wrap.ToDetailedString(index + 1, outed_contexts) + ";");
                    }
                    else if (x.Value is MemberAccess.ClassAccess ca)
                    {
                        var first = false;
                        foreach (var line in ca.ToString().Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (first) sb.Append("    ");
                            sb.AppendLine(line);
                            first = true;
                        }
                    }
                    else sb.AppendLine(Decoration(x.Value)?.ToString() ?? "null" + ";");
                }
                index -= 1;
                sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}}}");

                return sb.ToString();
            }

            public string ToShortString()
            {
                var sb = new StringBuilder();

                var index = 1;

                sb.AppendLine($"context {pred.Name} {{");
                foreach (var x in pred.LocalVariables.GetAsDictionary())
                {

                    sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}{x.Key} = ");
                    if (x.Value is ContextWrap wrap) sb.AppendLine($"context {wrap.pred.Name};");
                    else if (x.Value is MemberAccess.ClassAccess maca) sb.AppendLine($"access to {maca.Name.GetTypeString()};");
                    else sb.AppendLine((Decoration(x.Value)?.ToString() ?? "null") + ";");
                }
                index -= 1;
                sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}}}");
                return sb.ToString();
            }

            public override string ToString() => ToShortString();

            public object this[string index]
            {
                get => pred.LocalVariables.GetValue(index).Item1;
                set => pred.LocalVariables.SetValue(index, value);
            }

            public IEnumerator<object> GetEnumerator()
            {
                return pred.LocalVariables.Variables.Where(x => x != null).GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal ExecutionContext PreviousContext;
        //public ContextWrap pred => new ContextWrap(PreviousContext);
        public readonly ContextWrap wrap;
        internal ContextWrap upper;
        internal ExecutionContext toplevel { get => upper?.pred; set => upper = new ContextWrap(value); }

        public IEnumerable<ExecutionContext> GetHierarchy()
        {
            var context = this;
            while (context.toplevel != null)
            {
                yield return context.toplevel;
                context = context.toplevel;
            }
        }

        private int cycles = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartCycle()
        {
            cycles += 1;
            Broken = Continued = false;
        }

        public bool InCycle() => cycles > 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndCycle()
        {
            cycles -= 1;
            Broken = Continued = false;
        }

        public void Return() { Returned = true; ReturnedValue = null; }
        public void Return(object o) { Returned = true; ReturnedValue = o; }
        public void Break() { Broken = true; }
        public void Continue() { Continued = true; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareToInvoke() { Returned = Broken = Continued = false; }
        public void DefaultEnvironment()
        {
            
        }

        public BaseStatement parse(string s) => Parser.This.ParseScript(s);
        public object eval(IExecutable executable) => executable.GetValue(this);

        public LocalVariablesContainer LocalVariables { get; set; } = new LocalVariablesContainer();
    }
}
