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

        public override object GetValue(ExecutionContext context)
        {
            var right = Right.ToString().Replace(" ", "");
            if (right == "is") return Left;
            var type = right.ToType();
            if (type == null) throw new RuntimeError($"Type {right} not found", Right.SourceContext);
            return Left.GetValue(context).CastToType(type);
        }

        public override string Operator => "as";
    }
}
