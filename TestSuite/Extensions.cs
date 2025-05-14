using SLThree;
using SLThree.Extensions;
using System.Linq;
using System.Text;

namespace TestSuite
{
    public class Extensions
    {
        public static ExecutionContext GetOptionsFromStatements(BaseStatement statement)
        {
            if (statement is StatementList sl)
            {
                var optExpr = sl.Statements.FirstOrDefault(x => x is ExpressionStatement est && est.Expression is FunctionDefinition fd && fd.FunctionName is NameExpression fn && fn.Name == "options");

                if (optExpr == null) return null;

                var optFunc = ((optExpr as ExpressionStatement).Expression as FunctionDefinition);

                return ((optFunc?.GetValue(new ExecutionContext()) as Method)?.GetValue(new object[0]) as ContextWrap).Context;
            }
            return null;
        }

        public static (bool, string) GetDiff(string[] left, string[] right)
        {
            var sb = new StringBuilder();

            var table_height = left.Length + right.Length;

            var carriage_left = 0;
            var carriage_right = 0;

            var ret = true;

            while (carriage_left < left.Length && carriage_right < right.Length)
            {
                if (left[carriage_left] != right[carriage_right])
                {
                    sb.AppendLine($"{carriage_left + 1,4}: {left[carriage_left].Length,4}/{right[carriage_right].Length,4}");
                    ret = false;
                }
                carriage_left += 1; carriage_right += 1;
            }
            sb.AppendLine("=====OUTPUT\n" + right.JoinIntoString("\n") + "\n=====END OUTPUT");

            return (ret, sb.ToString());
        }
    }
}
