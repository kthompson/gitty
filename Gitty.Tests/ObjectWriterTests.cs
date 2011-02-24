using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gitty;
using Gitty.Storage;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    class ObjectWriterTests
    {
        [Test, TestCaseSource("Blobs")]
        public void ComputeBlobId(string filename, string expectedId)
        {
            using (Test.WorkingTree())
            {
                var file = new FileInfo(Path.Combine(Test.WorkingDirectory, filename));

                var id = ObjectWriter.ComputeId(new WorkingTreeFile(file, null, null, null));

                Assert.AreEqual(expectedId, id, "Testing file: "+ filename);
            }
        }

        [Test, TestCaseSource("Trees")]
        public void ComputeTreeId(string folder, string expectedId)
        {
            using (Test.WorkingTree())
            {
                var file = new DirectoryInfo(Path.Combine(Test.WorkingDirectory, folder));
                var tree = new WorkingTreeDirectory(file, null, null, null);

                
                var id = ObjectWriter.ComputeId(tree);

                Debug.WriteLine(string.Format("Expected Id: {0}\nId: {1}", expectedId, id));
                
                Assert.AreEqual(expectedId, id, "Testing folder: " + folder);
            }
        }

        [Test]
        public void DecompressTest()
        {
            using (Test.WorkingTree())
            {
                var file = Path.Combine(Test.WorkingDirectory, "011940f3815a5fd9079afb14279248ffb584ac88");
                var outfile = Path.Combine(Test.WorkingDirectory, "011940f3815a5fd9079afb14279248ffb584ac88_uncompressed");
                using (var cfile = new CompressionStream(file))
                {
                    using (var ufile = File.OpenWrite(outfile))
                    {
                        cfile.CopyTo(ufile);
                    }
                }
            }
        }

        public IEnumerable<TestCaseData> Trees()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.LsTree("-r", "-d", "HEAD");

                using (var reader = new StringReader(result))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(new[] { '\t' });
                        var name = parts[1];
                        var props = parts[0].Split(new[] { ' ' });
                        var id = props[2];

                        yield return new TestCaseData(name, id);
                    }
                }
            }
        }

        public IEnumerable<TestCaseData> Blobs()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.LsTree("-r", "HEAD");

                using (var reader = new StringReader(result))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(new[] { '\t' });
                        var name = parts[1];
                        var props = parts[0].Split(new[] { ' ' });
                        var id = props[2];

                        yield return new TestCaseData(name, id);
                    }
                }
            }
        }
    }
}
