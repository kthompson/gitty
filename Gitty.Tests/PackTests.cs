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

        public readonly string[] PackObjectIds = new[]
                                                     {
                                                         "d2b6e193ca7e1e2f27cc15a3f57ce15362cc2b88",
                                                         "ff2a2fc4b10c063092b3a19d1f78b2c94a79c231",
                                                         "ffc071d9a01d8fcd11cd6003b09bd619ddac18b4"
                                                     };

        public readonly int[] PackObjectOffsets = new[] {12, 3612, 5946};
        public readonly int[] PackObjectCrcs = new[] {-1780127250, -732676691, 2074085424};

        [Test, Sequential]
        public void IndexHasEntry(
            [ValueSource("PackObjectIds")]     string id,
            [ValueSource("PackObjectCrcs")] int crc,
            [ValueSource("PackObjectOffsets")]    int offset
            )
        {
            var file = Path.Combine(PacksPath, "pack-582fdcbadcd4640394f15127be4fb9e755876c51.idx");
            var index = new PackIndex(file, 237);
            var entry = index.GetEntry(id);

            Assert.AreEqual(id, entry.Id);
            Assert.AreEqual(crc, entry.Crc);
            Assert.AreEqual(offset, entry.Offset);
        }
    }
}
