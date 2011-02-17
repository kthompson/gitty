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
                msysGit = Test.Q(Test.GetTempFolder("msysgit"));
                var result = Test.Git.Init(msysGit);

                git = Test.GetTempFolder("gitty");
                Git.Init(git);

                Test.AssertFileSystemsSame(msysGit, git);
            }
            finally
            {
                if (msysGit != null) 
                    Test.DeleteRecursive(msysGit);

                if (git != null) 
                    Test.DeleteRecursive(git);
            }
        }

        [Test]
        public void GitInitBare()
        {
            string msysGit = null;
            string git = null;
            try
            {
                msysGit = Test.Q(Test.GetTempFolder("msysgit"));
                Test.Git.Init("--bare", msysGit);

                git = Test.GetTempFolder("gitty");
                Git.Init(git, true);

                Test.AssertFileSystemsSame(msysGit, git);
            }
            finally
            {
                if (msysGit != null)
                    Test.DeleteRecursive(msysGit);

                if (git != null)
                    Test.DeleteRecursive(git);
            }
        }

        [Test]
        public void EnumerateTreeItems()
        {
            using (Test.WorkingTree())
            {
                var treeId = "49055ddb2bad8335a11de37721de51419c455e2f";

                var result = Test.Git.LsTree("--name-only", treeId);

                var git = Git.Open(Test.WorkingDirectory);
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
        public void EnumerateTreeItemsRecursive([ValueSource("Trees")]string treeId)
        {
           using (Test.WorkingTree())
           {
               var result = Test.Git.LsTree("-r", "--name-only", treeId);

                var git = Git.Open(Test.WorkingDirectory);
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
                        var fullName = entry.FullName;
                        Assert.AreEqual(line, fullName);
                    }
                }
            }
        }

        public IEnumerable<string> Trees()
        {
            using (Test.WorkingTree())
            {
                var result = Test.Git.RevList("HEAD");
                var trees = new List<string>();
                var i = 0;
                using (var reader = new StringReader(result))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null && i++ < 20)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var commit = Test.Git.CatFile("commit", line);
                        var lines = commit.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                        var tree = lines[0].Substring(5);
                        yield return tree;
                    }
                }
            }
        }
    }
}
