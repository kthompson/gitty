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
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(this.Offset, SeekOrigin.Begin);
                
                LoadHeaderInfo(file);
                var position = file.Position;

                if(this.Type == "ofs_delta")
                {
                    LoadDelta(file, contentLoader);
                    return;
                }

                CompressedContentLoader(contentLoader)(file, this);
            }
        }

        private void LoadDelta(FileStream file, ContentLoader contentLoader)
        {
            var offset = this.Offset - file.Read7BitEncodedInt();
            var dataOffset = file.Position;

            var baseObject = this.PackFile.GetObjectLoader(offset);
            baseObject.Load((stream, loadInfo) =>
                                        {
                                            this.Type = loadInfo.Type;
                                            //this.Size = loadInfo.Size;
                                            if(contentLoader != null)
                                                contentLoader(stream, loadInfo);
                                        });
        }

        private void LoadHeaderInfo(Stream file)
        {
            var b = file.ReadByte();
            var type = (b & 0x70) >> 4;

            var size = b & 0xF;
            var bits = 4;

            while((b & 0x80) == 0x80)
            {
                b = file.ReadByte();
                size += (b & 0x7f) << bits;
                bits += 7;
            }

            this.Size = size;
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