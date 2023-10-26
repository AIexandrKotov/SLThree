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
        public CastLexem(BoxSupportedLexem castingLexem, BoxSupportedLexem castingType, Cursor cursor) : base(castingLexem, castingType, cursor) { }

        public override string ToString() => $"{Left} as {Right}";

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var right = Right.ToString().Replace(" ", "");
            if (right == "is")
            {
                reference = SLTSpeedyObject.GetAny(Left);
                return ref reference;
            }
            var type = right.ToType();
            if (type == null) throw new RuntimeError($"Type {right} not found", Right.SourceContext);
            {
                reference = SLTSpeedyObject.GetAny(Left.GetValue(context).CastToType(type));
                return ref reference;
            }
        }

        public override string Operator => "as";
    }
}
