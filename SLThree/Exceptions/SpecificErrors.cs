using System;

namespace SLThree
{
    /// <summary>
    /// Исключения во время работы программы
    /// </summary>
    [Serializable]
    public class AbstractInvokation : RuntimeError
    {
        public AbstractInvokation() : base() { }
        public AbstractInvokation(SourceContext context) : base(GetErrorText(), context) { }

        public static string GetErrorText()
        {
            return $"Trying to invoke abstract method";
        }
    }
}
