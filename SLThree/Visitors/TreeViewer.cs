using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLThree.Visitors
{
    public class TreeViewer : AbstractVisitor
    {
        public override void VisitStatement(StatementListStatement statement)
        {
            Level += 1;
            base.VisitStatement(statement);
            Level -= 1;
        }

        public override void VisitStatement(BaseStatement statement)
        {
            WriteTab();
            Writeln(statement.GetType().Name);
            WriteTab();
            WriteFiels(statement);
            Level += 1;
            base.VisitStatement(statement);
            Level -= 1;
        }

        public override void VisitExpression(BaseExpression expression)
        {
            WriteTab();
            Writeln(expression.GetType().Name);
            WriteTab();
            WriteFiels(expression);
            Level += 1;
            base.VisitExpression(expression);
            Level -= 1;
        }

        private object GetValue(object o)
        {
            if (o is IList<BaseExpression> lex) return $"[{lex.JoinIntoString(", ")}]";
            if (o is IList<BaseStatement> st) return $"[{st.Select(x => x.GetType().Name).JoinIntoString(", ")}]";
            return o;
        }
        private static List<string> WhiteList = new List<string>()
        {

        };
        public void WriteFiels(object o)
        {
            var d = new Dictionary<string, object>();
            var type = o.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
            var props = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
            foreach (var f in fields)
            {
                d[f.Name] = GetValue(f.GetValue(o));
            }
            foreach (var p in props)
            {
                d[p.Name] = GetValue(p.GetValue(o));
            }
            d.Remove("SourceContext");
            foreach (var key in d.Keys.ToArray())
                if (!WhiteList.Contains(key) && key.All(x => char.IsLower(x) || x == '_' || char.IsDigit(x)))
                    d.Remove(key);
            sb.AppendLine("//" + d.Where(x => !x.Key.Contains("<")).Select(x => $"{x.Key} = {x.Value}").JoinIntoString(", "));
        }
        public void WriteTab() => sb.Append(new string(' ', 4 * Level));
        public void Write(string s) => sb.Append(s);
        public void Writeln(string s) => sb.AppendLine(s);
        public StringBuilder sb = new StringBuilder();
        public int Level = 0;

        public static string GetView(object o)
        {
            var sv = new TreeViewer();
            sv.VisitAny(o);
            return sv.sb.ToString();
        }

        public override void Visit(Method method)
        {
            WriteTab();
            Writeln("\"REPRESENTATION OF METHOD\";");
            WriteTab();
            Writeln(method.ToString());
            base.Visit(method);
        }
    }
}
