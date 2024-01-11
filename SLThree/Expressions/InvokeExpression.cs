using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SLThree
{
    public class InvokeExpression : BaseExpression
    {
        public BaseExpression Left;
        public BaseExpression[] Arguments;

        public InvokeExpression(BaseExpression name, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Left = name;
            Arguments = arguments;
        }

        public override string ExpressionToString() => $"{Left}({Arguments.JoinIntoString(", ")})";

        private Func<object[], object[]> implicit_cached;

        /*private bool get_counted_name;
        private string get_name;*/
        public object GetValue(ExecutionContext context, object[] args)
        {
            var o = Left.GetValue(context);

            if (o == null) throw new RuntimeError($"Method {Left}(_) not found", SourceContext);

            if (o is Method method)
            {
                if (method.ParamNames.Length != args.Length) throw new RuntimeError("Call with wrong arguments count", SourceContext);
                return method.GetValue(context, args);
            }
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.Invoke(args[0], args.Skip(1).ToArray());
                else return mi.Invoke(null, args);
            }
            else if (o is ExecutionContext.IExecutable bl) return bl.GetValue(context);
            else
            {
                var type = o.GetType();
                type.GetMethods()
                    .FirstOrDefault(x => x.Name == Left.ExpressionToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Length)
                    ?.Invoke(o, args);
            }

            throw new RuntimeError("Unexecutable method", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, Arguments.ConvertAll(x => x.GetValue(context)));
        }

        private bool cached_1;
        private MethodInfo founded;
        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Left.Cast<NameExpression>().Name;

            if (cached_1) return founded.Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));

            if (obj is MemberAccess.ClassAccess ca)
            {
                ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static); 
                    // после первого вызова GetMethod
                    // переставляет перегрузки, у которых аргумент object
                    // в начало массива методов
                founded = ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length);
                cached_1 = true;
                if (founded == null) throw new RuntimeError($"Method {key}({Arguments.Select(x => "_").JoinIntoString(", ")}) not found", SourceContext);
                return founded.Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length)
                    .Invoke(obj, Arguments.ConvertAll(x => x.GetValue(context)));
            }

            return null;
        }

        public override object Clone()
        {
            return new InvokeExpression(Left.CloneCast(), Arguments.CloneArray(), SourceContext.CloneCast());
        }
    }
}
