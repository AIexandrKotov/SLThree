namespace SLThree
{
    public interface ILocalizedAssemblyInformation
    {
        string Name { get; }
        string Version { get; }
    }

    public interface ILanguageInformation : ILocalizedAssemblyInformation
    {
        string Edition { get; }
        LanguageInformation.IParser Parser { get; }
        LanguageInformation.IRestorator Restorator { get; }
    }
}