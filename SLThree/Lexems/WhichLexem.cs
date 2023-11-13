using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;

namespace SLThree
{
    public class WhichLexem : BaseLexem
    {
        public BaseLexem From;
        public BaseLexem Name;
        public TypeofLexem[] Types;

        public WhichLexem(BaseLexem from, BaseLexem name, TypeofLexem[] types, SourceContext context) : base(context)
        {
            From = from;
            Name = name;
            Types = types;

            method_name = Name.ToString();
        }

        public WhichLexem(BaseLexem name, TypeofLexem[] types, SourceContext context) : base(context)
        {
            Name = name;
            Types = types;

            var n = Name.ToString().Split('.');
            method_name = n.Last();
            type = n.Reverse().Skip(1).Reverse().JoinIntoString(".").ToType();
        }

        public override string ToString() =>
            From == null
            ? $"which({Name}<{Types.Select(x => x.GetType().GetTypeString() ?? "undefined").JoinIntoString(", ")}>)"
            : $"which({From}::{Name}<{Types.Select(x => x.GetType().GetTypeString() ?? "undefined").JoinIntoString(", ")}>)";

        private Type type;
        private string method_name;
        public override object GetValue(ExecutionContext context)
        {
            if (From == null)
                return type.GetMethod(method_name, Types.ConvertAll(x => (Type)x.GetValue(context)));
            else
            {
                var value = From.GetValue(context);
                if (value is MemberAccess.ClassAccess maca) value = maca.Name;
                var type = value as Type;
                return type.GetMethod(method_name, Types.ConvertAll(x => (Type)x.GetValue(context)));
            }
        }

        public override object Clone()
        {
            if (From == null)
                return new WhichLexem(Name.CloneCast(), Types.CloneArray(), SourceContext.CloneCast());
            else return new WhichLexem(From.CloneCast(), Name.CloneCast(), Types.CloneArray(), SourceContext.CloneCast());
        }
    }
}
