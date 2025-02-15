namespace SLThree
{
    public abstract class BaseInstanceCreator : BaseExpression
    {
        public override int Priority => -0xFF;

        public TypenameExpression Type;
        public BaseExpression Name;
        public BaseExpression[] Arguments;

        protected BaseInstanceCreator()
        {
        }

        protected BaseInstanceCreator(ISourceContext context) : base(context)
        {
        }
    }
}
