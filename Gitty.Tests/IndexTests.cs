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

    }
}
