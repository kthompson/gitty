using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gitty
{
    public abstract class ObjectLoader
    {
        public virtual ObjectType Type { get; protected set; }
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

        protected static ObjectType ObjectTypeFromString(string type)
        {
            return (ObjectType)Enum.Parse(typeof(ObjectType), type, true);
        }
    }
}