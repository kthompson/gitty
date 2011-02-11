using System.IO;

namespace Gitty.Storage
{
    class WholePackedObjectReader : PackedObjectReader
    {
        public WholePackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, int size, ObjectType type)
            : base(packFile, objectOffset, dataOffset, size, type)
        {

        }

        public override void Load(ContentLoader contentLoader = null)
        {
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(this.DataOffset, SeekOrigin.Begin);

                CompressedContentLoader(contentLoader)(file);
            }
        }
    }
}