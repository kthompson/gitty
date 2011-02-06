using System;
using System.IO;

namespace Gitty
{
    class PackedObjectLoader : ObjectLoader
    {
        public PackFile PackFile { get; private set; }
        public int? Offset { get; private set; }

        public PackedObjectLoader(PackFile packFile, string id, int? offset = null)
            : base(id)
        {
            this.PackFile = packFile;
            this.Offset = offset;
        }

        public override ObjectLoadInfo Load(ContentLoader contentLoader = null)
        {
            if (this.Offset == null)
            {
                var entry = this.PackFile.Index.GetEntry(this.Id);
                this.Offset = entry.Offset;
            }

            return LoadFromOffset(offset, contentLoader);
        }

        private ObjectLoadInfo LoadFromOffset(int offset, ContentLoader contentLoader)
        {
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(offset, SeekOrigin.Begin);
                
                var info = GetObjectInfo(file);

                if (contentLoader != null)
                {
                    //using (
                    //    var stream = new CompressionStream(file, System.IO.Compression.CompressionMode.Decompress,
                    //                                       true))
                    //{
                    contentLoader(file, info);
                    //}
                    
                }

                return info;
            }
        }

        private static ObjectLoadInfo GetObjectInfo(Stream file)
        {
            var b = file.ReadByte();
            var type = (b & 0x70) >> 4;

            var size = file.Read7BitEncodedInt(b & 0xF, 4);

            var typeString = GetTypeString(type);

            return new ObjectLoadInfo(typeString, size);
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