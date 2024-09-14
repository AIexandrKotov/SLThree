using SLThree.Metadata;

namespace SLThree.Language
{
    public class Restorator : DefaultRestorator, IRestorator
    {
        public override string LanguageName => "SLThree";
        public string Restore(BaseStatement statement, ExecutionContext context) => Restore<Restorator>(statement, context);
        public string Restore(BaseExpression expression, ExecutionContext context) => Restore<Restorator>(expression, context);
    }
}
