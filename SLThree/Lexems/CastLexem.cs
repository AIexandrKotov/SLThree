using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SLThree
{
    public class CastLexem : ExpressionBinary
    {
        public CastLexem(BaseLexem castingLexem, BaseLexem castingType, Cursor cursor) : base(castingLexem, castingType, cursor) { }

        public override string ToString() => $"{Left} as {Right}";

        private string name;
        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            if (name == null) name = Right.ToString().Replace(" ", "");
            if (name == "is") return Left;
            if (type == null) type = name.ToType();

            if (type == null) throw new RuntimeError($"Type \"{name}\" not found", Right.SourceContext);
            return Left.GetValue(context).CastToType(type);
        }

        public override string Operator => "as";
    }
}
