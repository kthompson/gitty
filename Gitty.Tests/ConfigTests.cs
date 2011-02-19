/*
 * Copyright (C) 2007, Dave Watson <dwatson@mimvista.com>
 * Copyright (C) 2008, Robin Rosenberg <robin.rosenberg@dewire.com>
 * Copyright (C) 2008, Shawn O. Pearce <spearce@spearce.org>
 * Copyright (C) 2008, Marek Zawirski <marek.zawirski@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class RepositoryConfigTest
    {
        [Test]
        public void Test001ReadBareKey()
        {
            Config c = Parse("[foo]\nbar\n");
            Assert.AreEqual(true, c.Read("foo", null, "bar", false));
            Assert.AreEqual(string.Empty, c.Read("foo", null, "bar"));
        }

        [Test]
        public void Test002ReadWithSubsection()
        {
            Config c = Parse("[foo \"zip\"]\nbar\n[foo \"zap\"]\nbar=false\nn=3\n");
            Assert.AreEqual(true, c.Read("foo", "zip", "bar", false));
            Assert.AreEqual(string.Empty, c.Read("foo", "zip", "bar"));
            Assert.AreEqual(false, c.Read("foo", "zap", "bar", true));
            Assert.AreEqual("false", c.Read("foo", "zap", "bar"));
            Assert.AreEqual(3, c.Read("foo", "zap", "n", 4));
            Assert.AreEqual(4, c.Read("foo", "zap", "m", 4));
        }

        [Test]
        public void Test003PutRemote()
        {
            var c = new Config();
            c.Write("sec", "ext", "name", "value");
            c.Write("sec", "ext", "name2", "value2");
            string expText = "[sec \"ext\"]\n\tname = value\n\tname2 = value2\n";
            Assert.AreEqual(expText, c.ToString());
        }

        [Test]
        public void Test004PutGetSimple()
        {
            var c = new Config();
            c.Write("my", null, "somename", "false");
            Assert.AreEqual("false", c.Read("my", null, "somename"));
            Assert.AreEqual("[my]\n\tsomename = false\n", c.ToString());
        }

        [Test]
        public void Test005PutReadListOfString()
        {
            var c = new Config();
            var values = new List<string> { "value1", "value2" };
            c.WriteList("my", null, "somename", values);

            string[] expArr = values.ToArray();
            var actArr = c.ReadList<string>("my", null, "somename");
            Assert.IsTrue(expArr.SequenceEqual(actArr));

            string expText = "[my]\n\tsomename = value1\n\tsomename = value2\n";
            Assert.AreEqual(expText, c.ToString());
        }

        [Test]
        public void Test006ReadCaseInsensitive()
        {
            Config c = Parse("[Foo]\nBar\n");
            Assert.AreEqual(true, c.Read("foo", null, "bar", false));
            Assert.AreEqual(string.Empty, c.Read("foo", null, "bar"));
        }

        [Test]
        public void TestReadBooleanTrueFalse1()
        {
            Config c = Parse("[s]\na = true\nb = false\n");
            Assert.AreEqual("true", c.Read("s", null, "a"));
            Assert.AreEqual("false", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test]
        public void TestReadBooleanTrueFalse2()
        {
            Config c = Parse("[s]\na = TrUe\nb = fAlSe\n");
            Assert.AreEqual("TrUe", c.Read("s", null, "a"));
            Assert.AreEqual("fAlSe", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test]
        public void TestReadBooleanYesNo1()
        {
            Config c = Parse("[s]\na = yes\nb = no\n");
            Assert.AreEqual("yes", c.Read("s", null, "a"));
            Assert.AreEqual("no", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test]
        public void TestReadBooleanYesNo2()
        {
            Config c = Parse("[s]\na = yEs\nb = NO\n");
            Assert.AreEqual("yEs", c.Read("s", null, "a"));
            Assert.AreEqual("NO", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test]
        public void testReadBoolean_OnOff1()
        {
            Config c = Parse("[s]\na = on\nb = off\n");
            Assert.AreEqual("on", c.Read("s", null, "a"));
            Assert.AreEqual("off", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test]
        public void testReadBoolean_OnOff2()
        {
            Config c = Parse("[s]\na = ON\nb = OFF\n");
            Assert.AreEqual("ON", c.Read("s", null, "a"));
            Assert.AreEqual("OFF", c.Read("s", null, "b"));

            Assert.IsTrue(c.Read("s", "a", false));
            Assert.IsFalse(c.Read("s", "b", true));
        }

        [Test, Sequential]
        public void TestReadLong([Values(1L, -1L, long.MinValue, long.MaxValue)]long value)
        {
            Config c = Parse("[s]\na = " + value + "\n");
            Assert.AreEqual(value, c.Read("s", null, "a", 0L));
        }

        [Test, Sequential]
        public void TestReadLongAbbreviated(
            [Values(4L * 1024 * 1024 * 1024, 3L * 1024 * 1024, 8L * 1024)] long value, 
            [Values("4g", "3 m", "8 k")] string name)
        {
            var c = Parse("[s]\na = " + name + "\n");
            Assert.AreEqual(value, c.Read("s", null, "a", 0L));
        }

        //[Test, ExpectedException(typeof(ArgumentException))]
        //public void TestReadLongWholeNumbersOnly()
        //{
        //    var c = Parse("[s]\na = 1.5g\n");
        //}

        [Test]
        public void TestBooleanWithNoValue()
        {
            Config c = Parse("[my]\n\tempty\n");
            Assert.AreEqual("", c.Read("my", null, "empty"));

            var list = c.ReadList<string>("my", null, "empty").ToArray();
            Assert.AreEqual(1, list.Length);
            Assert.AreEqual("", list[0]);

            Assert.IsTrue(c.Read("my", "empty", false));
            Assert.AreEqual("[my]\n\tempty\n", c.ToString());
        }

        [Test]
        public void TestEmptyString()
        {
            Config c = Parse("[my]\n\tempty =\n");
            Assert.IsNull(c.Read("my", null, "empty"));

            var values = c.ReadList<string>("my", null, "empty").ToArray();
            Assert.IsNotNull(values);
            Assert.AreEqual(1, values.Length);
            Assert.IsNull(values[0]);

            // always matches the default, because its non-boolean
            Assert.IsTrue(c.Read("my", "empty", true));
            Assert.IsFalse(c.Read("my", "empty", false));

            Assert.AreEqual("[my]\n\tempty =\n", c.ToString());

            c = new Config();
            c.WriteList("my", null, "empty", values.ToList());
            Assert.AreEqual("[my]\n\tempty =\n", c.ToString());
        }

        [Test]
        public void TestUnsetBranchSection()
        {
            Config c = Parse(""
            + "[branch \"keep\"]\n"
            + "  merge = master.branch.to.keep.in.the.file\n"
            + "\n"
            + "[branch \"remove\"]\n"
            + "  merge = this.will.get.deleted\n"
            + "  remote = origin-for-some-long-gone-place\n"
            + "\n"
            + "[core-section-not-to-remove-in-test]\n"
            + "  packedGitLimit = 14\n");
            c.DeleteSection("branch", "does.not.exist");
            c.DeleteSection("branch", "remove");
            Assert.AreEqual("" //
                    + "[branch \"keep\"]\n"
                    + "  merge = master.branch.to.keep.in.the.file\n"
                    + "\n"
                    + "[core-section-not-to-remove-in-test]\n"
                    + "  packedGitLimit = 14\n", c.ToString());
        }

        [Test]
        public void TestUnsetSingleSection()
        {
            Config c = Parse("" //
                    + "[branch \"keep\"]\n"
                    + "  merge = master.branch.to.keep.in.the.file\n"
                    + "\n"
                    + "[single]\n"
                    + "  merge = this.will.get.deleted\n"
                    + "  remote = origin-for-some-long-gone-place\n"
                    + "\n"
                    + "[core-section-not-to-remove-in-test]\n"
                    + "  packedGitLimit = 14\n");
            c.DeleteSection("single");
            Assert.AreEqual("" //
                    + "[branch \"keep\"]\n"
                    + "  merge = master.branch.to.keep.in.the.file\n"
                    + "\n"
                    + "[core-section-not-to-remove-in-test]\n"
                    + "  packedGitLimit = 14\n", c.ToString());
        }

        private static Config Parse(string content)
        {
            return Config.FromString(content);
        }
    }
}