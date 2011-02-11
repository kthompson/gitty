using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Gitty.Storage;

namespace Gitty
{
    public class Blob : TreeEntry
    {
        internal Blob(Repository repository, ObjectReader reader, string id, string name = null, string mode = null, Tree parent = null) 
            : base(repository, reader, id, name, mode, parent)
        {
        }

        public virtual void GetContentStream(Action<Stream, IObjectInfo> contentLoader)
        {
            this.Reader.Load(stream => contentLoader(stream, this.Reader));
        }

        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
    }
}
