﻿using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class UnaryGetChooser : UnaryOperator
    {
        public override string Operator => "*";
        public TypenameExpression Typename;
        public UnaryGetChooser(BaseExpression left, TypenameExpression typename, SourceContext context, bool priority = false) : base(left, context, priority)
        {
            Typename = typename;
        }
        public UnaryGetChooser() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            return Typename == null
                ? sys.random.to_chooser(Left.GetValue(context))
                : sys.random.to_chooser(Left.GetValue(context), Typename.GetValue(context).Cast<Type>());
        }
        public override object Clone()
        {
            return new UnaryGetChooser(Left.CloneCast(), Typename.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
