using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    public class ObjectLoadInfo
    {
        public string Type { get; private set; }
        public int Size { get; private set; }

        internal ObjectLoadInfo(string type, int size)
        {
            this.Type = type;
            this.Size = size;
        }
    }

    public abstract class ObjectLoader
    {
        public string Id { get; private set; }
        public Repository Repository { get; private set; }
        public abstract bool Exists { get; }

        protected ObjectLoader(Repository repository, string id)
        {
            this.Repository = repository;
            this.Id = id;
        }

        protected abstract Stream OpenStream();

        public delegate void ContentLoader(Stream stream, ObjectLoadInfo loadInfo);
        public ObjectLoadInfo Load(ContentLoader contentLoader = null)
        {
            
            var size = 0;
            var type = string.Empty;
            var inner = OpenStream();

            // this is a hack to get the DeflateStream to read properly
            inner.ReadByte();
            inner.ReadByte();

            using (var stream = new DeflateStream(inner, CompressionMode.Decompress))
            {
                var sb = new StringBuilder();
                var inHeader = true;

                while (inHeader)
                {
                    var c = (char) stream.ReadByte();
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
                if(contentLoader != null)
                    contentLoader(stream, loadInfo);
                return loadInfo;
            }
        }

        public static ObjectLoader Create(Repository repository, string id)
        {
            ObjectLoader loader = new LooseObjectLoader(repository, id);
            if (loader.Exists)
                return loader;

            loader = new PackedObjectLoader(repository, id);
            return loader.Exists ? loader : null;
        }

        public static ContentLoader DefaultContentLoader(Stream stream)
        {
            return (read, loadInfo) =>
                       {
                           var size = loadInfo.Size;
                           while (size > 0)
                           {
                               var bsize = size > 4096 ? 4096 : size;
                               var buffer = new byte[bsize];
                               var rsize = read.Read(buffer, 0, buffer.Length);
                               if (rsize == 0)
                                   return;

                               size -= rsize;
                               stream.Write(buffer, 0, rsize);
                           }
                           stream.Position = 0;
                       };
        }
    }
}