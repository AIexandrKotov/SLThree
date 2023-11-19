using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static SLThree.SwitchStatement;

namespace SLThree
{
    public class ReflectionLexem : BaseLexem
    {
        public BaseLexem From;
        public BaseLexem Name;
        public ReflectionLexem[] Types;
        public bool PropertyMode;

        public bool from_is_generic;
        public ReflectionLexem[] FromGenericArguments;

        public ReflectionLexem(BaseLexem from, ReflectionLexem[] from_generic, BaseLexem nameLexem, ReflectionLexem[] types, SourceContext context)
            : this(from, nameLexem, types, context)
        {
            from_is_generic = true;
            FromGenericArguments = from_generic;
        }
        public ReflectionLexem(BaseLexem from, ReflectionLexem[] from_generic, BaseLexem nameLexem, SourceContext context)
            : this(from, nameLexem, context)
        {
            from_is_generic = true;
            FromGenericArguments = from_generic;
        }
        public ReflectionLexem(BaseLexem from, ReflectionLexem[] from_generic, SourceContext context)
            : this(from, context)
        {
            from_is_generic = true;
            FromGenericArguments = from_generic;
        }

        public ReflectionLexem(BaseLexem from, BaseLexem nameLexem, ReflectionLexem[] types, SourceContext context) : base(context)
        {
            From = from;
            Name = nameLexem;
            Types = types;

            method_name = nameLexem.LexemToString().Replace(" ", "");
            PropertyMode = false;

            init_mode();
        }

        public ReflectionLexem(BaseLexem from, BaseLexem nameLexem, SourceContext context) : base(context)
        {
            From = from;
            Name = nameLexem;

            method_name = nameLexem.LexemToString().Replace(" ", "");
            PropertyMode = true;

            init_mode();
        }
        public ReflectionLexem(BaseLexem from, SourceContext context) : base(context)
        {
            From = from;
            Name = null;

            init_mode();
        }

        private void init_mode()
        {
            name = From.LexemToString().Replace(" ", "");
            mode = name == "\\" ? 2 : (name == "is" ? 1 : -1);
            if (mode == -1)
            {
                if (type == null) type = name.ToType();
                if (type == null) mode = 0;
            }
        }


        public override string LexemToString()
        {
            if (Name != null)
            {
                if (PropertyMode)
                {
                    return $"@{From}::{Name}";
                }
                else
                {
                    if (from_is_generic)
                    {
                        return $"@{From}<{FromGenericArguments.JoinIntoString(", ")}>::{Name}({Types.Select(x => x.ToString() ?? "undefined").JoinIntoString(", ")})";
                    }
                    else
                    {
                        return $"@{From}::{Name}({Types.Select(x => x.ToString() ?? "undefined").JoinIntoString(", ")})";
                    }
                }
            }
            else return $"@{From}";
        }

        public int mode = 0; // -1 - predefined, 0 - find type
        private string name;
        private string method_name;
        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            Type type_obj = null;
            if (mode == -1)
            {
                type_obj = type;
            }
            if (mode == 0)
            {
                var obj = From.GetValue(context);
                //if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", From.SourceContext);
                type_obj = obj is MemberAccess.ClassAccess maca ? maca.Name : (obj is Type t ? t : null);
                if (type_obj == null) throw new RuntimeError($"Type \"{name}\" not found", From.SourceContext);
            }

            if (Name != null)
            {
                return PropertyMode
                    ? (object)type_obj.GetProperty(method_name)
                    : type_obj.GetMethod(method_name, Types.ConvertAll(x => (Type)x.GetValue(context)));
            }
            else return type_obj;
        }

        public override object Clone()
        {
            return PropertyMode
                ? new ReflectionLexem(From.CloneCast(), Name.CloneCast(), Types.CloneArray(), SourceContext.CloneCast())
                : new ReflectionLexem(From.CloneCast(), Name.CloneCast(), SourceContext.CloneCast());
        }
    }
}
