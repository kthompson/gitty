using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    class LooseObjectLoader : ObjectLoader
    {
        public string Location { get; private set; }
        public long DataOffset { get; private set; }

        private LooseObjectLoader(string location, ObjectType type, int size, long dataOffset)
            : base(type, size)
        {
            this.Location = location;
            this.DataOffset = dataOffset;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            using (var stream = new CompressionStream(this.Location))
            {
                stream.SkipUntil(c => c == '\0');

                if (contentLoader != null)
                    contentLoader(stream);
            }
        }

        private static void ReadHeader(Stream stream, out ObjectType type, out int size)
        {
            size = 0;
            var typeCode = string.Empty;
            var sb = new StringBuilder();
            var inHeader = true;

            while (inHeader)
            {
                var c = (char)stream.ReadByte();
                switch (c)
                {
                    case ' ':
                        typeCode = sb.ToString();
                        sb.Clear();
                        continue;
                    case '\0':
                        size = int.Parse(sb.ToString());
                        sb.Clear();
                        inHeader = false;
                        continue;
                }
                sb.Append(c);
            }

            type = ObjectTypeFromString(typeCode);
        }

        public static LooseObjectLoader GetObjectLoader(string objectsLocation, string id)
        {
            var location = Path.Combine(objectsLocation, id.Substring(0, 2), id.Substring(2));

            if (File.Exists(location))
            {
                var inner = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var stream = new CompressionStream(inner, CompressionMode.Decompress))
                {
                    ObjectType type;
                    int size;
                    ReadHeader(stream, out type, out size);
                    var dataOffset = inner.Position;
                    return new LooseObjectLoader(location, type, size, dataOffset);
                }
            }
                

            return null;
        }
    }
}