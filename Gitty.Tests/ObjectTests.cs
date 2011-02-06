using System;
using System.Collections.Generic;
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
                var git = Git.Open(TestHelper.WorkingDirectory);
                var blob = git.OpenObject(id);

                Assert.IsAssignableFrom<Blob>(blob);
            }
        }
    }
}
