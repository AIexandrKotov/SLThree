using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class ContextStatement : BaseStatement
    {
        public NameExpression Name;
        public BaseExpression Cast;
        public BaseStatement[] Body;

        public bool HasBody => Body.Length > 0;

        public ContextStatement(NameExpression name, SourceContext context) : base(context)
        {
            Name = name;
            Body = new BaseStatement[0];
        }

        public ContextStatement(NameExpression name, BaseStatement[] body, SourceContext context) : base(context)
        {
            Name = name;
            Body = body;
        }

        public ContextStatement(NameExpression nameExpression, BaseExpression cast, BaseStatement[] body, SourceContext context) : base(context)
        {
            Name = nameExpression;
            Cast = cast;
            Body = body;

            if (cast != null)
            {
                name = Cast.ToString().Replace(" ", "");
                mode = name == "\\" ? 2 : -1;
                if (mode == -1)
                {
                    if (type == null) type = name.ToType();
                    if (type == null) mode = 0;
                }
            }
        }

        public int mode = 0; // -1 - predefined, 0 - find type
        public bool variable_assigned = false;
        public int variable_index;

        private string name;
        private Type type;

        public override string ToString() => $"context {Name.Name}{(Cast!=null?$": {Cast}":"")} {{\n{Body.JoinIntoString("\n")}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var wrap = new ExecutionContext.ContextWrap(new ExecutionContext(context));
            if (Name != null) wrap.pred.Name = Name.Name;
            object ret = wrap;
            if (HasBody)
            {
                for (var i = 0; i < Body.Length; i++)
                {
                    if (Body[i] is ExpressionStatement es && es.Expression is ExpressionBinaryAssign assign)
                        assign.AssignValue(wrap.pred, assign.Left, assign.Right.GetValue(context));
                    else if (Body[i] is ContextStatement cs)
                        cs.GetValue(wrap.pred);
                }
            }
            if (Cast != null)
            {
                if (mode == 0)
                {
                    var obj = Cast.GetValue(context);
                    if (obj == null) throw new RuntimeError($"Type \"{name}\" not found", SourceContext);
                    if (obj is Type tp) ret = wrap.CastToType(tp);
                    else ret = wrap.CastToType((obj as MemberAccess.ClassAccess).Name);
                }
                else
                {
                    ret = wrap.CastToType(type);
                }
            }
            context.LocalVariables.SetValue(wrap.pred.Name, ret);
            return ret;
        }

        public override object Clone() => new ContextStatement(Name.CloneCast(), Cast.CloneCast(), Body.CloneCast(), SourceContext.CloneCast());
    }
}
