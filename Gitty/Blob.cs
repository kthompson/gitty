using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Gitty.Storage;

namespace Gitty
{
    public class Blob : TreeEntry
    {
        internal Blob(string id, long size, Func<byte[]> loader, Tree parent = null, string name = null, string mode = null)
            : base(id, parent, name, mode)
        {
            _loader = new Lazy<byte[]>(loader);
            this.Size = size;
        }

        public long Size { get; private set; }

        private string _id;
        public override string Id
        {
            get { return base.Id ?? _id ?? (_id = ObjectWriter.ComputeId(this)); }
        }

        private readonly Lazy<byte[]> _loader;
        public byte[] Data
        {
            get { return _loader.Value; }
        }

        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
    }
}
