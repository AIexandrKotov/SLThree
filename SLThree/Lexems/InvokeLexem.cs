using Pegasus.Common;
using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SLThree
{
    public class InvokeLexem : BaseLexem
    {
        public BaseLexem Name;
        public BaseLexem[] Arguments;

        public InvokeLexem(BaseLexem name, BaseLexem[] arguments, Cursor cursor) : base(cursor)
        {
            Name = name;
            Arguments = arguments;
        }

        public override string ToString() => $"{Name}({Arguments.JoinIntoString(", ")})";

        private bool get_counted_name;
        private string get_name;
        public object GetValue(ExecutionContext context, object[] args)
        {
            if (!get_counted_name)
            {
                get_name = Name.ToString().Replace(" ", "");
                get_counted_name = true;
            }
            var o = context.LocalVariables.GetValue(get_name).Item1;

            if (o == null) throw new RuntimeError($"Method {get_name}(_) not found", SourceContext);

            if (o is BaseLexem bl) return bl.GetValue(context);
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.Invoke(args[0], args.Skip(1).ToArray());
                else return mi.Invoke(null, args);
            }
            else if (o is Method method)
            {
                if (method.ParamNames.Length != args.Length) throw new RuntimeError("Call with wrong arguments count", SourceContext);
                return method.GetValue(context, args);
            }
            else
            {
                var type = o.GetType();
                type.GetMethods()
                    .FirstOrDefault(x => x.Name == Name.ToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Length)
                    ?.Invoke(o, args);
            }

            return null;
        }

        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, Arguments.ConvertAll(x => x.GetValue(context)));
        }

        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Name.ToString().Replace(" ", "");

            if (obj is MemberAccess.ClassAccess ca)
            {
                return ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length)
                    .Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length)
                    .Invoke(obj, Arguments.ConvertAll(x => x.GetValue(context)));
            }

            return null;
        }
    }
}
