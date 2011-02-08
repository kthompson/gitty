using System;
using System.IO;
using System.IO.Compression;

namespace Gitty
{
    class WholePackedObjectLoader : PackedObjectLoader
    {
        public WholePackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, int size, ObjectType type)
            : base(packFile, objectOffset, dataOffset, size, type)
        {

        }

        public override void Load(ContentLoader contentLoader = null)
        {
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(this.DataOffset, SeekOrigin.Begin);

                CompressedContentLoader(contentLoader)(file, this);
            }
        }
    }
}