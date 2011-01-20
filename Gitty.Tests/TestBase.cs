using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty.Tests
{
    public class TestBase
    {
        public static readonly string ArtifactsPath = Path.Combine("..", "..", "Artifacts");
        public static readonly string ObjectsPath = Path.Combine(ArtifactsPath, "objects");
        public static readonly string SampleRepo = Path.Combine(ArtifactsPath, "sample_repo");
        public static readonly string SampleRepoGit = Path.Combine(ArtifactsPath, "sample_repo.git");

        public static string GetObjectAsString(string type, string id)
        {
            if (!type.EndsWith("s"))
                type = type + "s";

            var path = Path.Combine(ObjectsPath, type, id);

            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
