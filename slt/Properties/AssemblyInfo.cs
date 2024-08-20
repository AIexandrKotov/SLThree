using System;

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

    public const string FullName = "SLThree REPL";
    public const string Name = "slt";
}
