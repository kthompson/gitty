using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    public abstract class ObjectLoader
    {
        public string Type { get; protected set; }
        public long Size { get; protected  set; }

        public delegate void ContentLoader(Stream stream, ObjectLoader loader);

        public abstract void Load(ContentLoader contentLoader = null);

        public static ObjectLoader Create(Repository repository, string id)
        {
            var loader = LooseObjectLoader.GetObjectLoader(repository.ObjectsLocation, id);
            if (loader != null)
                return loader;

            //TODO: we should be caching the pack files and using FileSystemWatcher or something for updates
            var pf = PackFile.FindAll(repository).Where(pack => pack.HasEntry(id)).FirstOrDefault();
            if (pf != null)
                return pf.GetObjectLoader(id);

            return null;
        }

        public static ContentLoader CompressedContentLoader(ContentLoader loader)
        {
            if (loader == null)
                return (stream, objectLoader) => { };

            return (stream, objectLoader) =>
                       {
                           using (
                               var compressed = new CompressionStream(stream,
                                                                      CompressionMode.Decompress,
                                                                      true))
                           {
                               loader(compressed, objectLoader);
                           }
                       };
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