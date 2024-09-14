namespace SLThree
{
    public abstract class BaseInstanceCreator : BaseExpression
    {
        public TypenameExpression Type;
        public BaseExpression Name;
        public BaseExpression[] Arguments;

        protected BaseInstanceCreator()
        {
        }

        protected BaseInstanceCreator(ISourceContext context) : base(context)
        {
        }
        protected BaseInstanceCreator(bool priority, ISourceContext context) : base(priority, context)
        {
        }
    }
}
