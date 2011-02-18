using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace Gitty.Storage
{
    /// <summary>
    /// A class to read objects from different storages
    /// </summary>
    public abstract class ObjectReader 
    {
        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual ObjectType Type { get; protected set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public virtual long Size { get; protected set; }

        /// <summary>
        /// A delegate used to deal with loading data
        /// </summary>
        /// <param name="stream">The stream.</param>
        public delegate void ContentLoader(Stream stream);

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectReader"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="size">The size.</param>
        protected ObjectReader(ObjectType type, long size)
        {
            this.Type = type;
            this.Size = size;
        }

        /// <summary>
        /// Loads the specified content loader.
        /// </summary>
        /// <param name="contentLoader">The content loader.</param>
        public abstract void Load(ContentLoader contentLoader = null);


        /// <summary>
        /// Creates a compressed ContentLoader.
        /// </summary>
        /// <param name="loader">The loader.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets an ObjectType from a string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected static ObjectType ObjectTypeFromString(string type)
        {
            return (ObjectType)Enum.Parse(typeof(ObjectType), type, true);
        }
    }
}