using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class SampleRepoTests : TestBase
    {
        [Test]
        public void RepositoryHasRefs()
        {
            var git = Git.Open(null, SampleRepoGit);
            var refs = git.Refs.ToDictionary(r => r.Name, r => r);

            Assert.AreEqual(5, refs.Count);

            Assert.That(refs.ContainsKey("master"));
            Assert.That(refs.ContainsKey("branch1"));
            Assert.That(refs.ContainsKey("branch_to_merge"));
            Assert.That(refs.ContainsKey("remotes/origin/master"));
            Assert.That(refs.ContainsKey("initial_release"));

            AssertRef(refs["master"], "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", false, "master", null, git, RefType.Head, "refs/heads/master");
            AssertRef(refs["branch1"], "e4582d4a976101a2bb6ecfff0e22b03d19fa90a1", false, "branch1", null, git, RefType.Head, "refs/heads/branch1");
            AssertRef(refs["branch_to_merge"], "39513d9ec02031479db870bec97250d7f5171962", false, "branch_to_merge", null, git, RefType.Head, "refs/heads/branch_to_merge");
            AssertRef(refs["remotes/origin/master"], "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", false, "remotes/origin/master", "origin", git, RefType.Remote, "refs/remotes/origin/master");
            AssertRef(refs["initial_release"], "56ffce8543f0840854ce9ca6f6fa55719e15e0d6", false, "initial_release", null, git, RefType.Tag, "refs/tags/initial_release");
        }

        private static void AssertRef(Ref r, string id, bool isPacked, string name, string remoteName, Repository repository, RefType refType, string relativePath)
        {
            Assert.AreEqual(id, r.Id);
            Assert.AreEqual(isPacked, r.IsPacked);
            Assert.AreEqual(name, r.Name);
            Assert.AreEqual(remoteName, r.RemoteName);
            Assert.AreEqual(refType, r.Type);
            Assert.AreEqual(relativePath, r.RelativePath);
        }

        [Test]
        public void RepositoryHasHeads()
        {
            var git = Git.Open(null, SampleRepoGit);
            var refs = git.Heads.ToDictionary(r => r.Name, r => r);

            Assert.AreEqual(3, refs.Count);

            Assert.That(refs.ContainsKey("master"));
            Assert.That(refs.ContainsKey("branch1"));
            Assert.That(refs.ContainsKey("branch_to_merge"));

            AssertRef(refs["master"], "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", false, "master", null, git, RefType.Head, "refs/heads/master");
            AssertRef(refs["branch1"], "e4582d4a976101a2bb6ecfff0e22b03d19fa90a1", false, "branch1", null, git, RefType.Head, "refs/heads/branch1");
            AssertRef(refs["branch_to_merge"], "39513d9ec02031479db870bec97250d7f5171962", false, "branch_to_merge", null, git, RefType.Head, "refs/heads/branch_to_merge");
            
        }

        [Test]
        public void RepositoryHeadsAreBranches()
        {
            var git = Git.Open(null, SampleRepoGit);
            var heads = git.Heads.ToList();
            var branches = git.Branches.ToList();
            
            Assert.AreEqual(heads.Count, branches.Count);

            for (int i = 0; i < heads.Count; i++)
            {
               Assert.AreEqual(heads[i].Id, branches[i].Id);
            }
        }

        [Test]
        public void RepositoryHasRemotes()
        {
            var git = Git.Open(null, SampleRepoGit);
            var refs = git.Remotes.ToDictionary(r => r.Name, r => r);

            Assert.AreEqual(1, refs.Count);

            Assert.That(refs.ContainsKey("remotes/origin/master"));

            AssertRef(refs["remotes/origin/master"], "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", false, "remotes/origin/master", "origin", git, RefType.Remote, "refs/remotes/origin/master");
        }

        [Test]
        public void RepositoryHasTags()
        {
            var git = Git.Open(null, SampleRepoGit);
            var refs = git.Tags.ToDictionary(r => r.Name, r => r);

            Assert.AreEqual(1, refs.Count);

            Assert.That(refs.ContainsKey("initial_release"));

            AssertRef(refs["initial_release"], "56ffce8543f0840854ce9ca6f6fa55719e15e0d6", false, "initial_release", null, git, RefType.Tag, "refs/tags/initial_release");
        }

        [Test]
        public void RepositoryHasHead()
        {
            var git = Git.Open(null, SampleRepoGit);
            var head = git.Head;

            Assert.AreEqual(false, head.IsDetached);

            Assert.AreEqual("f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", head.Id);
            var r = head.Ref;
            Assert.NotNull(r);

            AssertRef(r, "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", false, "master", null, git, RefType.Head, "refs/heads/master");
        }

        [Test]
        public void LoadObjectWithLooseObjectLoader()
        {
            var git = Git.Open(null, SampleRepoGit);
            var head = git.Head;

            Assert.AreEqual(false, head.IsDetached);

            Assert.AreEqual("f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", head.Id);

            var loader = ObjectLoader.Create(git, head.Id);
            Assert.NotNull(loader);

            var info = loader.Load((stream, loadInfo) =>
            {
                Assert.AreEqual(189, loadInfo.Size);
                Assert.AreEqual("commit", loadInfo.Type);

                string actual;
                using (var reader = new StreamReader(stream))
                {
                    actual = reader.ReadToEnd();
                }

                var expected = GetObjectAsString("commit", "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8");

                Assert.AreEqual(expected, actual);
            });

            Assert.AreEqual(189, info.Size);
            Assert.AreEqual("commit", info.Type);

        }

        [Test]
        public void RepositoryCanOpenObjectCommit()
        {
            var git = Git.Open(null, SampleRepoGit);
            var id = "f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8";
            var obj = git.OpenObject(id);

            Assert.IsInstanceOf(typeof(Commit), obj);

            var commit = obj as Commit;
            if (commit == null)
                Assert.Fail();

            Assert.AreEqual(id, commit.Id);

            Assert.AreEqual("Kevin Thompson <mrunleaded@gmail.com> 1295412656 -0800", commit.Author);

            Assert.AreEqual("Kevin Thompson <mrunleaded@gmail.com> 1295412656 -0800", commit.Commiter);

            Assert.NotNull(commit.Tree);
            Assert.AreEqual("7ee9583cd8b390caac802ece6c144314ef5fc3bf", commit.Tree.Id);

        }

        [Test]
        public void RepositoryCanOpenObjectTree()
        {
            var git = Git.Open(null, SampleRepoGit);
            var id = "7ee9583cd8b390caac802ece6c144314ef5fc3bf";
            var obj = git.OpenObject(id);

            Assert.IsInstanceOf(typeof(Tree), obj);

            var tree = obj as Tree;
            if (tree == null)
                Assert.Fail();

            Assert.AreEqual(id, tree.Id);
            Assert.AreEqual(1, tree.Items.Count());
            var treeItem = tree.Items.FirstOrDefault();
            Assert.NotNull(treeItem);

            Assert.AreEqual("100644", treeItem.Mode);
            Assert.AreEqual("blob", treeItem.Type);
            Assert.AreEqual("0f4a22329fb3970ca4c19d873623c68e937ba16c", treeItem.Id);
            Assert.AreEqual("README", treeItem.Name);
        }

        [Test]
        public void RepositoryCanOpenObjectTag()
        {
            var git = Git.Open(null, SampleRepoGit);
            var id = "56ffce8543f0840854ce9ca6f6fa55719e15e0d6";
            var obj = git.OpenObject(id);

            Assert.IsInstanceOf(typeof(Tag), obj);

            var tag = obj as Tag;
            if (tag == null)
                Assert.Fail();

            Assert.AreEqual(id, tag.Id);

            Assert.AreEqual("commit", tag.Type);
            Assert.IsInstanceOf(typeof(Commit), tag.Object);

            var commit = tag.Object as Commit;
            if(commit == null)
                Assert.Fail();

            Assert.AreEqual("f5f1da3d5aa6aa03479df730c64d5525e5d6d5d8", commit.Id);

            Assert.AreEqual("initial_release", tag.Name);
            Assert.AreEqual("Kevin Thompson <mrunleaded@gmail.com> 1295412704 -0800", tag.Tagger);

            Assert.AreEqual("this is a tag\n", tag.Message);
        }

        [Test]
        public void RepositoryCanOpenObjectBlob()
        {
            var git = Git.Open(null, SampleRepoGit);
            var id = "0f4a22329fb3970ca4c19d873623c68e937ba16c";
            var obj = git.OpenObject(id);

            Assert.IsInstanceOf(typeof(Blob), obj);

            var blob = obj as Blob;
            if (blob == null)
                Assert.Fail();

            Assert.AreEqual(id, blob.Id);

            blob.GetContentStream((stream, info) =>
            {
                Assert.AreEqual(47, info.Size);
                Assert.AreEqual("blob", info.Type);

                string actual;
                using (var reader = new StreamReader(stream))
                {
                    actual = reader.ReadToEnd();
                }

                var expected = GetObjectAsString("blob", id);

                Assert.AreEqual(expected, actual);

            });
        }

        [Test]
        public void RepositoryOpenObjectReturnsNullForInvalidIds()
        {
            var git = Git.Open(null, SampleRepoGit);
            var id = "abcdefghijklmnopqrstuvwxyz01234567890123";
            var obj = git.OpenObject(id);

            Assert.IsNull(obj);
        }
    }
}
