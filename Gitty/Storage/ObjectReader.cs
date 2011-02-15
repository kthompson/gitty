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