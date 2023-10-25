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
        public IList<BaseLexem> Arguments;

        public InvokeLexem(BaseLexem name, IList<BaseLexem> arguments, Cursor cursor) : base(cursor)
        {
            Name = name;
            Arguments = arguments;
        }

        public override string ToString() => $"{Name}({Arguments.JoinIntoString(", ")})";

        public object GetValue(ExecutionContext context, object[] args)
        {
            var o = context.LocalVariables[Name.ToString().Replace(" ", "")];

            if (o is BaseLexem bl) return bl.GetValue(context);
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.Invoke(args[0], args.Skip(1).ToArray());
                else return mi.Invoke(null, args);
            }
            else if (o is Method method)
            {
                return method.GetValue(context, Arguments.Select(x => x.GetValue(context)).ToArray());
            }
            else
            {
                var type = o.GetType();
                type.GetMethods()
                    .FirstOrDefault(x => x.Name == Name.ToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Count)
                    ?.Invoke(o, args);
            }

            return null;
        }

        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, Arguments.Select(x => x.GetValue(context)).ToArray());
        }

        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Name.ToString().Replace(" ", "");

            if (obj is MemberAccess.ClassAccess ca)
            {
                return ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Count)
                    .Invoke(null, Arguments.Select(x => x.GetValue(context)).ToArray());
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Count)
                    .Invoke(obj, Arguments.Select(x => x.GetValue(context)).ToArray());
            }

            return null;
        }
    }
}
