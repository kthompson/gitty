using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    class LooseObjectLoader : ObjectLoader
    {
        public string Location { get; private set; }

        internal LooseObjectLoader(Repository repository, string id)
            : base(repository, id)
        {
            this.Location = Path.Combine(repository.Location, "objects", id.Substring(0, 2), id.Substring(2));
        }

        public override ObjectLoadInfo Load(ContentLoader contentLoader = null)
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
                var loadInfo = new ObjectLoadInfo(type, size);
                if (contentLoader != null)
                    contentLoader(stream, loadInfo);
                return loadInfo;
            }
        }
    }
}