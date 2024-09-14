

namespace SLThree
{
    public class UnexpectedError : LexicalError
    {
        private static string To_str(char c)
        {
            switch (c)
            {
                case '\r': return "\\r";
                case '\n': return "\\n";
                case '\t': return "\\t";
                default: return c.ToString();
            }
        }
        public UnexpectedError(char c, ISourceContext cursor) : base(string.Format(Locale.Current["ERR_Unexpected"], To_str(c)), cursor) { }
    }
}
