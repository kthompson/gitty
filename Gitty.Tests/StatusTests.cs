using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class StatusTests
    {

        [Test]
        public void EnumerateTreeItems()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.Status("-s", "-u", "--ignored");

                var git = Git.Open(Test.WorkingDirectory);
                Assert.NotNull(git);

                var status = git.Status;
                Assert.NotNull(status);

            }
        }
    }
}
