using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    class LooseObjectLoader : ObjectLoader
    {
        public string Location { get; private set; }

        private LooseObjectLoader(string location)
        {
            this.Location = location;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            var size = 0;
            var type = string.Empty;
            var inner = new FileStream(this.Location, FileMode.Open, FileAccess.Read, FileShare.Read);

            using (var stream = new CompressionStream(inner, CompressionMode.Decompress))
            {
                var sb = new StringBuilder();
                var inHeader = true;

                while (inHeader)
                {
                    var c = (char)stream.ReadByte();
                    switch (c)
                    {
                        case ' ':
                            type = sb.ToString();
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
                this.Type = type;
                this.Size = size;

                if (contentLoader != null)
                    contentLoader(stream, this);

            }
        }

        public static LooseObjectLoader GetObjectLoader(string objectsLocation, string id)
        {
            var location = Path.Combine(objectsLocation, id.Substring(0, 2), id.Substring(2));

            if (File.Exists(location))
                return new LooseObjectLoader(location);

            return null;
        }
    }
}