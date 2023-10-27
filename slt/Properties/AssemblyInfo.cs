using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Общие сведения об этой сборке предоставляются следующим набором
// набора атрибутов. Измените значения этих атрибутов для изменения сведений,
// связанные с этой сборкой.
[assembly: AssemblyTitle(REPLVersion.FullName)]
[assembly: AssemblyDescription(REPLVersion.FullName)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(REPLVersion.FullName)]
[assembly: AssemblyCopyright(SLTVersion.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// из модели COM задайте для атрибута ComVisible этого типа значение true.
[assembly: ComVisible(false)]

// Следующий GUID представляет идентификатор typelib, если этот проект доступен из модели COM
[assembly: Guid("22884539-6522-4fa4-a3af-18b92030a41d")]

// Сведения о версии сборки состоят из указанных ниже четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии
//      Номер сборки
//      Номер редакции
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(REPLVersion.Version)]
[assembly: AssemblyFileVersion(REPLVersion.Version)]

public static class REPLVersion
{
    public const string Major = "1"; //vh
    public const string Minor = "0"; //vh
    public const string Build = "0"; //vh
    public const string Revision = "183"; //vh
    public const long LastUpdate = 638340102434683730; //vh

    public const string Version = Major + "." + Minor + "." + Build + "." + Revision;
    public const string VersionWithoutRevision = Major + "." + Minor + "." + Build;
    public const string VersionRevisionInBrackets = Major + "." + Minor + "." + Build + " (" + Revision + ")";
    public const string FullName = "SLThree REPL";
    public const string Name = "slt";
}
