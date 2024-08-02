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

        protected BaseInstanceCreator(SourceContext context) : base(context)
        {
        }
        protected BaseInstanceCreator(bool priority, SourceContext context) : base(priority, context)
        {
        }
    }
}
