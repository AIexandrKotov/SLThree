using SLThree.Extensions;
using System;
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
    public class Reflected
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }
        public DateTime LastUpdate { get; private set; }

        public string Version { get; private set; }
        public string VersionWithoutRevision { get; private set; }

        public Reflected()
        {
            var slt_version = typeof(REPLVersion);
            Major = int.Parse(slt_version.GetField("Major").GetValue(null).Cast<string>());
            Minor = int.Parse(slt_version.GetField("Minor").GetValue(null).Cast<string>());
            Build = int.Parse(slt_version.GetField("Build").GetValue(null).Cast<string>());
            Revision = int.Parse(slt_version.GetField("Revision").GetValue(null).Cast<string>());
            LastUpdate = new DateTime(slt_version.GetField("LastUpdate").GetValue(null).Cast<long>());

            Version = Major + "." + Minor + "." + Build + "." + Revision;
            VersionWithoutRevision = Major + "." + Minor + "." + Build;
        }
    }
    public const string Major = "1"; //vh
    public const string Minor = "2"; //vh
    public const string Build = "0"; //vh
    public const string Revision = "218"; //vh
    public const long LastUpdate = 638415391372079738; //vh

    public const string Version = Major + "." + Minor + "." + Build + "." + Revision;
    public const string VersionWithoutRevision = Major + "." + Minor + "." + Build;
    public const string VersionRevisionInBrackets = Major + "." + Minor + "." + Build + " (" + Revision + ")";
    public const string FullName = "SLThree REPL";
    public const string Name = "slt";
}
