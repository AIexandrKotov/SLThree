namespace SLThree
{
    public abstract class Special : BaseExpression
    {
        public Special()
        {
        }

        public Special(SourceContext context) : base(context)
        {
        }

        public Special(bool priority, SourceContext context) : base(priority, context)
        {
        }
    }
}
