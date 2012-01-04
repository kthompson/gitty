using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    class GitIgnoreTests
    {
        [Test]
        public void ShouldIgnoreFile()
        {
            //TODO: figure out how to hook up gitignore
            var ignore = new GitIgnore();
            ignore.Excludes.Add("bin");
            ignore.Excludes.Add("obj");

            var root = new TestTree(null, "root") 
            { 
                new TestTreeEntry("hi"),
                new TestTreeEntry("obj", ObjectType.Tree)
                {
                    new TestTreeEntry("something.o")       
                },
                new TestTreeEntry("bin", ObjectType.Tree)
                {
                    new TestTreeEntry("something.exe"),
                    new TestTreeEntry("something.pdb")       
                },
                new TestTreeEntry("files", ObjectType.Tree)
                {
                    new TestTreeEntry("file1"),
                    new TestTreeEntry("file2")       
                },
                new TestTreeEntry("readme.txt")
            };

            var expectedEntries = new[] { "files/file1", "files/file2", "root/hi", "root/readme.txt" };
            var i = 0;
            var items = root.EnumerateItems(true).ToArray();

            Assert.AreEqual(expectedEntries.Length, items.Length, "Number of Entries");
            foreach(var entry in items)
            {
                Assert.AreEqual(expectedEntries[i++], entry.FullName);
            }
        }

    }
}
