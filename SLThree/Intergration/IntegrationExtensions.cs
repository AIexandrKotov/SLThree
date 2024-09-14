using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Intergration
{
    public static class IntegrationExtensions
    {
        public static ExecutionContext RunScript(this string code, string scriptName = null)
        {
            return Parser.This.RunScript(code, scriptName);
        }
        public static ExecutionContext RunScript(this string code, ExecutionContext context, string scriptName = null)
        {
            return Parser.This.RunScript(code, scriptName, context);
        }
        public static ExecutionContext RunScript(this ExecutionContext context, string code, string scriptName = null)
        {
            return Parser.This.RunScript(code, scriptName, context);
        }
        public static ExecutionContext RunPreset(this string code, ExecutionContext preset, string scriptName = null)
        {
            return Parser.This.RunScript(code, scriptName, null, preset);
        }
    }
}
