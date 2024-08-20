using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class SLTVersion
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
            var slt_version = typeof(SLTVersion);
            var version = slt_version.Assembly.GetName().Version;
            Major = version.Major;
            Minor = version.Minor;
            Build = version.Build;
            Revision = version.Revision;
            //LastUpdate = new DateTime(slt_version.GetField("LastUpdate").GetValue(null).Cast<long>());

            Version = Major + "." + Minor + "." + Build + "." + Revision;
            VersionWithoutRevision = Major + "." + Minor + "." + Build;
        }
    }

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

    public static string Edition { get; } = "Massive Update";

    public static string GetTitle()
    {
        if (string.IsNullOrEmpty(Edition)) return Name;
        else return $"{Name} {Edition}";
    }
}
