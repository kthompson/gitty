using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class ObjectTests
    {
        [Test]
        public void RepositoryCanLoadDeltaObjects(
            [Values("b1f187c2e9acaba942639bca90a63c5b4f058967")]string id)
        {
            using (Test.WorkingTree())
            {
                var content = Test.Git.CatFile("blob", id);

                var git = Git.Open(Test.WorkingDirectory);
                var o = git.OpenObject(id);

                Assert.IsAssignableFrom<Blob>(o);
                var blob = o as Blob;

                Assert.NotNull(blob);

                var actual = Test.ReadToEndOfStream(new MemoryStream(blob.Data));
                Assert.AreEqual(content, actual);
            }
        }
    }
}
