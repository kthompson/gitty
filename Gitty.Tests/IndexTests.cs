using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    class IndexTests 
    {

        [Test]
        public void IndexHasValidHeader()
        {
            var index = new Index(Path.Combine(Test.ArtifactsPath, "index"));
            Assert.AreEqual(83, index.EntryCount);
            Assert.AreEqual(2, index.Version);
        }

        [Test]
        public void IndexAlwaysHasSameEntryCount()
        {
            var index = new Index(Path.Combine(Test.ArtifactsPath, "index"));
            Assert.AreEqual(83, index.EntryCount);
            Assert.AreEqual(83, index.Entries.Count());
            Assert.AreEqual(83, index.EntryCount);
            Assert.AreEqual(83, index.Entries.Count());
            Assert.AreEqual(83, index.EntryCount);
            Assert.AreEqual(83, index.Entries.Count());
        }

        [Test,ExpectedException(typeof(InvalidOperationException))]
        public void IndexDoesntLikeNonIndexes()
        {
            var index = new Index(Path.Combine(Test.ObjectsPath, "blobs", "0f4a22329fb3970ca4c19d873623c68e937ba16c"));
            //force load
            var version = index.EntryCount;
        }

        [Test]
        public void IndexedFilesMatchIndexCount()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.LsFiles();

                var git = Git.Open(Test.WorkingDirectory);
                Assert.NotNull(git);
                var index = git.Index;
                
                using (var reader = new StringReader(result))
                {
                    foreach (var entry in index.Entries)
                    {
                        var line = reader.ReadLine();
                        Assert.AreEqual(line, entry.Name);
                    }
                }
            }
        }

        [Test]
        public void GetWorkingDirectory()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.LsFiles("-s");

                var git = Git.Open(Test.WorkingDirectory);
                Assert.NotNull(git);
                var index = git.Status.WorkingTree;

                using (var reader = new StringReader(result))
                {
                    foreach (var entry in index.EnumerateItems(true))
                    {
                        var line = reader.ReadLine();
                        var id = entry.Id;
                        if (line == id)
                        {
                        }
                    }
                }
            }
        }
    }
}
