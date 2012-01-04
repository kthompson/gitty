using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    public static class Test
    {
        public static readonly string ArtifactsPath = Path.Combine("..", "..", "Artifacts");
        public static readonly string ObjectsPath = Path.Combine(ArtifactsPath, "objects");
        public static readonly string SampleRepo = Path.Combine(ArtifactsPath, "sample_repo");
        public static readonly string SampleRepoGit = Path.Combine(ArtifactsPath, "sample_repo.git");

        public static readonly string WorkingDirectory;

        static Test()
        {
            var gittyRoot = Environment.GetEnvironmentVariable("GITTY_ROOT");
            if(gittyRoot == null)
                throw new InvalidOperationException("GITTY_ROOT was not specified. Please make sure to set the GITTY_ROOT environment variable to the root of your Gitty checkout directory.");

            WorkingDirectory  = new DirectoryInfo(gittyRoot).FullName;

            ArtifactsPath = Path.Combine(WorkingDirectory, "Gitty.Tests", "Artifacts");
            ObjectsPath = Path.Combine(ArtifactsPath, "objects");
            SampleRepo = Path.Combine(ArtifactsPath, "sample_repo");
            SampleRepoGit = Path.Combine(ArtifactsPath, "sample_repo.git");
        }

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

        public static dynamic Git
        {
            get { return new MSysGit(); }
        }

        public static string RandomString(string prefix = "", int length = 40)
        {
            var sb = new StringBuilder(length);
            sb.Append(prefix);
            while(sb.Length < length)
            {
                var temp = Guid.NewGuid().ToString().Replace("-", "");
                sb.Append(temp);
            }

            return sb.ToString(0, length);
        }

        public static string GetTempFolder(string prefix = "", int length = 40)
        {
            var path = Path.Combine(Path.GetTempPath(), RandomString(prefix, length));// +Path.DirectorySeparatorChar;
            Directory.CreateDirectory(path);
            return path;
        }

        public static void AssertFileSystemsSame(string system1, string system2, string pattern = "*")
        {
            system1 = NQ(system1);
            system2 = NQ(system2);

            var msysGitUri = GetUri(system1);

            var gitUri = GetUri(system2);

            var msysGitEntries = Directory.EnumerateFileSystemEntries(system1, pattern, SearchOption.AllDirectories);
            var gitEntries = Directory.EnumerateFileSystemEntries(system2, pattern, SearchOption.AllDirectories);
            var enumerator = gitEntries.GetEnumerator();
            foreach (var msysGitEntry in msysGitEntries)
            {
                Assert.IsTrue(enumerator.MoveNext());
                var mgeRel = msysGitUri.MakeRelativeUri(new Uri(msysGitEntry));
                var gitEntry = enumerator.Current;

                var gitRel = gitUri.MakeRelativeUri(new Uri(gitEntry));
                Assert.AreEqual(mgeRel.ToString(), gitRel.ToString());

                if (File.Exists(gitEntry) && File.Exists(msysGitEntry))
                    AssertFilesSame(gitEntry, msysGitEntry);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        private static Uri GetUri(string uri)
        {
            uri = EnsureSlash(uri);

            return new Uri(uri);
        }

        private static string EnsureSlash(string uri)
        {
            if (!uri.EndsWith(Path.DirectorySeparatorChar.ToString()))
                uri += Path.DirectorySeparatorChar;

            return uri;
        }

        private static string NQ(string s)
        {
            const string q = "\"";
            return s.StartsWith(q) && s.EndsWith(q) ? s.Substring(1, s.Length - 2) : s;
        }

        public static void AssertFilesSame(string file1, string file2)
        {
            // open first file 
            var stream1 = new FileStream(file1, System.IO.FileMode.Open);
            var stream2 = new FileStream(file2, System.IO.FileMode.Open);

            Assert.AreEqual(stream1.Length, stream2.Length);
            
            AssertFilesSame(stream1, stream2);
        }

        public static void AssertFilesSame(Stream stream1, Stream stream2)
        {
            using (var f1 = stream1)
            {
                using (var f2 = stream2)
                {
                    while (true)
                    {
                        var i = f1.ReadByte();
                        var j = f2.ReadByte();

                        Assert.AreEqual(i, j);

                        if (i == -1 || j == -1)
                            return;
                    }
                }
            }
        }

        public static bool DeleteRecursive(string folder)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    File.Delete(file);
                }

                foreach (var subfolder in Directory.EnumerateDirectories(folder))
                {
                    DeleteRecursive(subfolder);
                }

                Directory.Delete(folder);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IDisposable WorkingTree(string workingTree = null)
        {
            Environment.SetEnvironmentVariable("GIT_WORK_TREE", workingTree ?? WorkingDirectory, EnvironmentVariableTarget.Process);
            return new Temp(() => Environment.SetEnvironmentVariable("GIT_WORK_TREE", string.Empty, EnvironmentVariableTarget.Process));
        }

        public static string ReadToEndOfStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Q(string value)
        {
            return "\"" + value + "\"";
        }

        class Temp : IDisposable
        {
            private readonly Action _disposeAction;

            public Temp(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                _disposeAction();
            }
        }
    }
}
