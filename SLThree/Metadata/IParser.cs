namespace SLThree.Metadata
{
    public interface IParser
    {
        BaseStatement ParseScript(string code, string fileName);
        BaseExpression ParseExpression(string code, string fileName);
    }
}
