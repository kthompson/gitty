using System;
using System.IO;

namespace Gitty
{
    class PackedObjectLoader : ObjectLoader
    {
        public PackFile PackFile { get; private set; }
        public long Offset { get; private set; }

        private ObjectLoader _base;

        public PackedObjectLoader(PackFile packFile, long offset)
        {
            this.PackFile = packFile;
            this.Offset = offset;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            LoadFromOffset(this.Offset, contentLoader);
        }

        private void LoadFromOffset(long offset, ContentLoader contentLoader)
        {
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(offset, SeekOrigin.Begin);
                
                LoadHeaderInfo(file);
                var position = file.Position;

                if(this.Type == "ofs_delta")
                {
                    LoadDelta(file, contentLoader);
                }
                
                if (contentLoader != null)
                {
                    //using (
                    //    var stream = new CompressionStream(file, System.IO.Compression.CompressionMode.Decompress,
                    //                                       true))
                    //{
                    contentLoader(file, this);
                    //}
                    
                }
            }
        }

        private void LoadDelta(FileStream file, ContentLoader contentLoader)
        {
            var offset = file.Read7BitEncodedInt();
            var baseObject = this.PackFile.GetObjectLoader(offset);
            baseObject.Load((stream, loadInfo) =>
                                        {
                                            this.Type = loadInfo.Type;
                                            this.Size = loadInfo.Size;
                                        });
        }

        private void LoadHeaderInfo(Stream file)
        {
            var b = file.ReadByte();
            var type = (b & 0x70) >> 4;

            this.Size = file.Read7BitEncodedInt(b & 0xF, 4);
            this.Type = GetTypeString(type);
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