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
    }
}
