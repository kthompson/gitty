using System;
using System.IO;

namespace Gitty
{
    class PackedObjectLoader : ObjectLoader
    {
        public PackFile PackFile { get; private set; }

        public PackedObjectLoader(PackFile packFile, string id)
            : base(id)
        {
            this.PackFile = packFile;
        }

        public override ObjectLoadInfo Load(ContentLoader contentLoader = null)
        {
            var entry = this.PackFile.Index.GetEntry(this.Id);

            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(entry.Offset, SeekOrigin.Begin);
                
                var info = GetObjectInfo(file);

                if (contentLoader != null)
                {
                    using (
                        var stream = new CompressionStream(file, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        contentLoader(stream, info);
                    }
                }

                return info;
            }
        }

        private static ObjectLoadInfo GetObjectInfo(Stream file)
        {
            var b = file.ReadByte();
            var type = (b & 0x70) >> 4;

            var size = GetSize(file, b);

            var typeString = GetTypeString(type);

            return new ObjectLoadInfo(typeString, size);
        }

        private static int GetSize(Stream file, int b)
        {
            var size = (b & 0xF);
            var sizeBits = 4;
            while (true)
            {
                if ((b & 0x80) != 0x80)
                    break;

                b = file.ReadByte();
                size |= (b & 0x7F) << sizeBits;
                sizeBits += 7;
            }
            return size;
        }

        private static string GetTypeString(int type)
        {
            switch (type)
            {
                case 1:
                    return "commit";
                case 2:
                    return "tree";
                case 3:
                    return "blob";
                case 4:
                    return "tag";
                case 6: 
                    return "ofs_delta";
                case 7:
                    return "ref_delta";
                default:
                    throw new InvalidOperationException(string.Format("invalid type: {0}", type));
            }
        }
    }

    //V2 PackIndex
}