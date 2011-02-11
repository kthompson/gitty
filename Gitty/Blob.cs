using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Gitty.Storage;

namespace Gitty
{
    public class Blob : AbstractObject
    {
        public long Size { get; private set; }

        internal Blob(string id, long size, Func<byte[]> loader) 
            : base(ObjectType.Blob, id)
        {
            _loader = new Lazy<byte[]>(loader);
            this.Size = size;
        }

        private readonly Lazy<byte[]> _loader;
        public byte[] Data
        {
            get { return _loader.Value; }
        }
    }
}
