using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace Gitty.Storage
{
    public abstract class ObjectReader : IObjectInfo
    {
        public virtual ObjectType Type { get; protected set; }
        public long Size { get; protected  set; }

        public delegate void ContentLoader(Stream stream);

        protected ObjectReader(ObjectType type, long size)
        {
            this.Type = type;
            this.Size = size;
        }

        public abstract void Load(ContentLoader contentLoader = null);

        public static ObjectReader Create(Repository repository, string id)
        {
            var loader = LooseObjectReader.GetObjectLoader(repository.ObjectsLocation, id);
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
                return stream => { };

            return stream =>
                       {
                           using (
                               var compressed = new CompressionStream(stream,
                                                                      CompressionMode.Decompress,
                                                                      true))
                           {
                               loader(compressed);
                           }
                       };
        }

        protected static ObjectType ObjectTypeFromString(string type)
        {
            return (ObjectType)Enum.Parse(typeof(ObjectType), type, true);
        }
    }

    public interface IObjectInfo
    {
        ObjectType Type { get;}
        long Size { get; }
    }
}