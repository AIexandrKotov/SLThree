using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLThree
{
    public class ContextWrap : IEnumerable<object>
    {
        public readonly ExecutionContext Context;

        public ContextWrap(ExecutionContext context)
        {
            this.Context = context;
        }

        public static Func<object, object> Decoration = o => o;

        public string ToDetailedString(int index, List<ContextWrap> outed_contexts)
        {
            var sb = new StringBuilder();
            outed_contexts.Add(this);

            sb.AppendLine($"context {Context.Name} {{");
            foreach (var x in Context.LocalVariables.GetAsDictionary())
            {

                sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}{x.Key} = ");
                if (x.Value is ContextWrap wrap)
                {
                    if (outed_contexts.Contains(wrap)) sb.AppendLine($"context {wrap.Context.Name}; //already printed");
                    else sb.AppendLine(wrap.ToDetailedString(index + 1, outed_contexts) + ";");
                }
                else if (x.Value is ClassAccess ca)
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

            sb.AppendLine($"context {Context.Name} {{");
            foreach (var x in Context.LocalVariables.GetAsDictionary())
            {

                sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}{x.Key} = ");
                if (x.Value is ContextWrap wrap) sb.AppendLine($"context {wrap.Context.Name};");
                else if (x.Value is ClassAccess maca) sb.AppendLine($"access to {maca.Name.GetTypeString()};");
                else sb.AppendLine((Decoration(x.Value)?.ToString() ?? "null") + ";");
            }
            index -= 1;
            sb.Append($"{(index == 0 ? "" : new string(' ', index * 4))}}}");
            return sb.ToString();
        }

        public override string ToString() => ToShortString();

        public object this[string index]
        {
            get => Context.LocalVariables.GetValue(index).Item1;
            set => Context.LocalVariables.SetValue(index, value);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Context.LocalVariables.Variables.Where(x => x != null).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj)
        {
            return Context.Equals(obj is ContextWrap wrap ? wrap.Context : null);
        }
    }
}
