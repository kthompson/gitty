using System;
using System.IO;

namespace Gitty
{
    class PackedObjectLoader : ObjectLoader
    {
        internal PackedObjectLoader(Repository repository, string id)
            : base(repository, id)
        {
        }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        protected override Stream OpenStream()
        {
            throw new NotImplementedException();
        }
    }
}