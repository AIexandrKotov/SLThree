using Pegasus.Common;
using SLThree.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace SLThree
{
    public class InvokeLexem : BoxSupportedLexem
    {
        public BaseLexem Name;
        public IList<BaseLexem> Arguments;

        public InvokeLexem(BaseLexem name, IList<BaseLexem> arguments, Cursor cursor) : base(cursor)
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
            var o = context.LocalVariables.GetValue(get_name).Item1.Boxed();

            if (o is BaseLexem bl) return bl.GetValue(context);
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.Invoke(args[0], args.Skip(1).ToArray());
                else return mi.Invoke(null, args);
            }
            else if (o is Method method)
            {
                return method.GetValue(context, Arguments.Select(x => x.Cast<BoxSupportedLexem>().GetBoxValue(context)).ToArray());
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

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            return ref GetValue(context, Arguments.Select(x => x.GetValue(context).Boxed()).ToArray()).ToSpeedy(ref reference);
            throw new System.NotImplementedException();
        }

        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Name.ToString().Replace(" ", "");

            if (obj is MemberAccess.ClassAccess ca)
            {
                return ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Count)
                    .Invoke(null, Arguments.Select(x => x.GetValue(context).Boxed()).ToArray());
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Count)
                    .Invoke(obj, Arguments.Select(x => x.GetValue(context).Boxed()).ToArray());
            }

            return null;
        }
    }
}
