using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    class HeadTests
    {
        [Test]
        public void HasValidHead()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.RevParse("HEAD");
                var commitId = result.ToString().Trim();

                result = Test.Git.RevParse("HEAD^{tree}");
                var treeId = result.ToString().Trim();

                var git = Git.Open(Test.WorkingDirectory);
                Assert.NotNull(git);

                var head = git.Head;
                Assert.NotNull(head);
                Assert.AreEqual(commitId, head.Id);

                var commit = git.Head.Commit;
                Assert.NotNull(commit);
                Assert.AreEqual(commitId, commit.Id);

                var tree = commit.Tree;
                Assert.NotNull(tree);
                Assert.AreEqual(treeId, tree.Id);
            }
        }
    }
}
