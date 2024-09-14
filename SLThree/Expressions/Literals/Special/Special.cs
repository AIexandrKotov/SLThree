namespace SLThree
{
    public abstract class Special : BaseExpression
    {
        public Special()
        {
        }

        public Special(ISourceContext context) : base(context)
        {
        }

        public Special(bool priority, ISourceContext context) : base(priority, context)
        {
        }
    }
}
