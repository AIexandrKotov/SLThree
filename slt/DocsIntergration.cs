using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace slt
{
    public class DocsIntergration
    {
        public static Dictionary<string, string[]> VersionsData { get; private set; }
        public static string[] Specification { get; private set; }
        public static string[] Help { get; private set; }

        internal static string[] ReadStrings(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
            }
        }

        static DocsIntergration()
        {
            var SLTAssembly = Assembly.GetExecutingAssembly();
            VersionsData = SLTAssembly
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("slt.docs.versions."))
                .ToDictionary(
                    x => Path.GetFileName(x).Replace("slt.docs.versions.", ""),
                    x => { using (var stream = SLTAssembly.GetManifestResourceStream(x)) return ReadStrings(stream); }
                );
            using (var stream = SLTAssembly.GetManifestResourceStream("slt.docs.specification"))
            {
                Specification = ReadStrings(stream);
            }
            using (var stream = SLTAssembly.GetManifestResourceStream("slt.docs.help"))
            {
                Help = ReadStrings(stream);
            }
        }
    }
}
