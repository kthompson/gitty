using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty.Storage
{
    class LooseObjectReader : ObjectReader
    {
        public string Location { get; private set; }
        public long DataOffset { get; private set; }

        private LooseObjectReader(string location, ObjectType type, int size, long dataOffset)
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

        public static LooseObjectReader GetObjectLoader(string objectsLocation, string id)
        {
            var location = ObjectExists(objectsLocation, id);
            if (location == null)
                return null;

            var path = Path.Combine(objectsLocation, id.Substring(0, 2), id.Substring(2));

            using (var inner = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var stream = new CompressionStream(inner);
                ObjectType type;
                int size;
                ReadHeader(stream, out type, out size);
                var dataOffset = inner.Position;
                return new LooseObjectReader(location, type, size, dataOffset);
            }
        }

        private static string ObjectExists(string objectsLocation, string id)
        {
            var location = Path.Combine(objectsLocation, id.Substring(0, 2), id.Substring(2));
            if(File.Exists(location))
                return location;

            return null;

        }
    }
}