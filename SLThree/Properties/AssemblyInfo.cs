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
    public const string Minor = "1"; //vh
    public const string Build = "1"; //vh
    public const string Revision = "168"; //vh
    public const long LastUpdate = 638336881507973753; //vh

    public const string Version = Major + "." + Minor + "." + Build + "." + Revision;
    public const string VersionWithoutRevision = Major + "." + Minor + "." + Build;
    public const string VersionRevisionInBrackets = Major + "." + Minor + "." + Build + " (" + Revision + ")";
    public const string Name = "SLThree";
    public const string Author = "Alexandr Kotov";
    public const string Copyright = Author + " 2023";

    public static string Edition { get; } = "Lovely 64 bits";

    public static string GetTitle()
    {
        if (string.IsNullOrEmpty(Edition)) return Name;
        else return $"{Name} {Edition}";
    }
}
