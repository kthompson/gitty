using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gitty.Tests
{
    [TestFixture]
    public class PackTests : TestBase
    {
        public static readonly string PacksPath = Path.Combine(ArtifactsPath, "packs");

        public readonly string[] PackObjectIds = new[] { "d2b6e193ca7e1e2f27cc15a3f57ce15362cc2b88" };
        public readonly int[] PackObjectOffsets = new[] { 12 };
        public readonly int[] PackObjectCrcs = new[] { -1780127250 };

        [Test, Sequential]
        public void IndexHasEntry(
            [ValueSource("PackObjectIds")]     string id,
            [ValueSource("PackObjectOffsets")] int crc,
            [ValueSource("PackObjectCrcs")]    int offset
            )
        {
            var file = Path.Combine(PacksPath, "pack-582fdcbadcd4640394f15127be4fb9e755876c51.idx");
            var index = new PackIndex(file, 237);
            var entry = index.GetEntry(id);

            Assert.AreEqual(crc, entry.Crc);
            Assert.AreEqual(id, entry.Id);
            Assert.AreEqual(offset, entry.Offset);
        }
    }
}
