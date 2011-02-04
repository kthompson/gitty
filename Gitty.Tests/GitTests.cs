using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class GitTests
    {

        [Test]
        public void GitInit()
        {
            string msysGit = null;
            string git = null;
            try
            {
                msysGit = TestHelper.GetTempFolder("msysgit");
                TestHelper.Git.Init(msysGit);

                git = TestHelper.GetTempFolder("gitty");
                Git.Init(git);

                TestHelper.AssertFileSystemsSame(msysGit, git);
            }
            finally
            {
                if (msysGit != null) 
                    TestHelper.DeleteRecursive(msysGit);

                if (git != null) 
                    TestHelper.DeleteRecursive(git);
            }
        }

        [Test]
        public void GitInitBare()
        {
            string msysGit = null;
            string git = null;
            try
            {
                msysGit = TestHelper.GetTempFolder("msysgit");
                TestHelper.Git.Init("--bare", msysGit);

                git = TestHelper.GetTempFolder("gitty");
                Git.Init(git, true);

                TestHelper.AssertFileSystemsSame(msysGit, git);
            }
            finally
            {
                if (msysGit != null)
                    TestHelper.DeleteRecursive(msysGit);

                if (git != null)
                    TestHelper.DeleteRecursive(git);
            }
        }

        [Test]
        public void EnumerateTreeItems()
        {
            var workingDirectory = new DirectoryInfo(Path.Combine("..", "..", "..")).FullName;

            using (TestHelper.WorkingTree(workingDirectory))
            {
                //Environment.SetEnvironmentVariable("GIT_WORK_TREE", workingDirectory);
                //Environment.SetEnvironmentVariable("GIT_DIR", Path.Combine(workingDirectory, ".git"));

                var treeId = "49055ddb2bad8335a11de37721de51419c455e2f";

                var result = TestHelper.Git.LsTree("--name-only", treeId);

                var git = Git.Open(workingDirectory);
                Assert.NotNull(git);

                var o = git.OpenObject(treeId);
                Assert.NotNull(o);

                Assert.IsAssignableFrom<Tree>(o);

                var tree = o as Tree;
                Assert.NotNull(tree);

                using (var reader = new StringReader(result))
                {
                    foreach (var entry in tree.EnumerateItems())
                    {
                        var line = reader.ReadLine();
                        Assert.AreEqual(line, entry.Name);
                    }
                }
            }
        }

        [Test]
        public void EnumerateTreeItemsRecursive()
        {
            var workingDirectory = new DirectoryInfo(Path.Combine("..", "..", "..")).FullName;

            using (TestHelper.WorkingTree(workingDirectory))
            {
                //Environment.SetEnvironmentVariable("GIT_WORK_TREE", workingDirectory);
                //Environment.SetEnvironmentVariable("GIT_DIR", Path.Combine(workingDirectory, ".git"));

                var treeId = "49055ddb2bad8335a11de37721de51419c455e2f";

                var result = TestHelper.Git.LsTree("-r", "--name-only", treeId);

                var git = Git.Open(workingDirectory);
                Assert.NotNull(git);

                var o = git.OpenObject(treeId);
                Assert.NotNull(o);

                Assert.IsAssignableFrom<Tree>(o);

                var tree = o as Tree;
                Assert.NotNull(tree);

                using (var reader = new StringReader(result))
                {
                    foreach (var entry in tree.EnumerateItems(true))
                    {
                        var line = reader.ReadLine();
                        var name = entry.Name;
                        Assert.AreEqual(line, name);
                    }
                }
            }
        }
    }
}
