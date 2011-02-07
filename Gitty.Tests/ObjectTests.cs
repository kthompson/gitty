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
        public void RepositoryCanLoadDeltaObjects()
        {
            using(TestHelper.WorkingTree())
            {
                var id = "b1f187c2e9acaba942639bca90a63c5b4f058967";
                var bytes = Encoding.ASCII.GetBytes(TestHelper.Git.CatFile("blob", id));
                var file = new MemoryStream(bytes);
                

                var git = Git.Open(TestHelper.WorkingDirectory);
                var o = git.OpenObject(id);

                Assert.IsAssignableFrom<Blob>(o);
                var blob = o as Blob;

                Assert.NotNull(blob);

                blob.GetContentStream((stream, loader) => TestHelper.AssertFilesSame(stream, file));
            }
        }
    }
}
