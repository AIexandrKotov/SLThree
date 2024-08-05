using System;

namespace SLThree.Visitors
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class VisitorNoticeAttribute : Attribute
    {
        public string MethodName;
        public Type MethodArg;

        public VisitorNoticeAttribute(string methodName, Type methodArg)
        {
            MethodName = methodName;
            MethodArg = methodArg;
        }
    }
}
