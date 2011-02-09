using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace Gitty
{
    public class Blob : TreeEntry
    {
        internal Blob(Repository repository, ObjectLoader loader, string id, string name = null, string mode = null, Tree parent = null) 
            : base(repository, loader, id, name, mode, parent)
        {
        }

        public void GetContentStream(Action<Stream, IObjectInfo> contentLoader)
        {
            this.Loader.Load(stream => contentLoader(stream, this.Loader));
        }

        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
    }
}
