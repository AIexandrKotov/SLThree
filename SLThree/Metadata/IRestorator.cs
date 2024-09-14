namespace SLThree.Metadata
{
    public interface IRestorator
    {
        string Restore(BaseStatement statement, ExecutionContext context);
        string Restore(BaseExpression expression, ExecutionContext context);
    }
}
