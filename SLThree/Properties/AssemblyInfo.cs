using System.IO;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

// Общие сведения об этой сборке предоставляются следующим набором
// набора атрибутов. Измените значения этих атрибутов для изменения сведений,
// связанные со сборкой.
[assembly: AssemblyTitle(SLTVersion.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(SLTVersion.Name)]
[assembly: AssemblyCopyright(SLTVersion.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("06a97274-8266-4f25-a78b-8cf615b12771")]

// Сведения о версии сборки состоят из указанных ниже четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии
//      Номер сборки
//      Редакция
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(SLTVersion.Version)]
[assembly: AssemblyFileVersion(SLTVersion.Version)]

public static class SLTVersion {
    public const string Major = "0"; //vh
    public const string Minor = "2"; //vh
    public const string Build = "0"; //vh
    public const string Revision = "762"; //vh
    public const long LastUpdate = 638345449084390873; //vh

    public const string Version = Major + "." + Minor + "." + Build + "." + Revision;
    public const string VersionWithoutRevision = Major + "." + Minor + "." + Build;
    public const string VersionRevisionInBrackets = Major + "." + Minor + "." + Build + " (" + Revision + ")";
    public const string Name = "SLThree";
    public const string Author = "Alexandr Kotov";
    public const string Copyright = Author + " 2023";

    internal static string[] ReadStrings(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
        }
    }
    public static SortedDictionary<string, string[]> VersionsData { get; private set; }
    public static string[] Specification { get; private set; }
    static SLTVersion()
    {
        var ass = Assembly.GetExecutingAssembly();
        VersionsData = new SortedDictionary<string, string[]>(ass
            .GetManifestResourceNames()
            .Where(x => x.StartsWith("SLThree.docs.versions."))
            .ToDictionary(
                x => Path.GetFileName(x).Replace("SLThree.docs.versions.", ""),
                x => { using (var stream = ass.GetManifestResourceStream(x)) return ReadStrings(stream); }
            ));
        using (var stream = ass.GetManifestResourceStream("SLThree.docs.specification"))
        {
            Specification = ReadStrings(stream);
        }
    }

    public static string Edition { get; } = "Unwrap the wrap";

    public static string GetTitle()
    {
        if (string.IsNullOrEmpty(Edition)) return Name;
        else return $"{Name} {Edition}";
    }
}
