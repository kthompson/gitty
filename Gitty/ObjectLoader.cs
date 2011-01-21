using System.Linq;
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

        protected ObjectLoader(string id)
        {
            this.Id = id;
        }

        public delegate void ContentLoader(Stream stream, ObjectLoadInfo loadInfo);

        public abstract ObjectLoadInfo Load(ContentLoader contentLoader = null);

        public static ObjectLoader Create(Repository repository, string id)
        {
            var loader = new LooseObjectLoader(repository.ObjectsLocation, id);
            if (File.Exists(loader.Location))
                return loader;

            var pf = PackFile.FindAll(repository).Where(pack => pack.HasEntry(id)).FirstOrDefault();
            if (pf != null)
                return pf.GetObjectLoader(id);

            return null;
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