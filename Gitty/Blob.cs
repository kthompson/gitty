using System;
using System.Linq;
using System.Security.AccessControl;

namespace Gitty
{
    public class Blob : ITreeEntry
    {
        private readonly ObjectLoader _loader;

        public string Id { get; private set; }

        internal Blob(ObjectLoader loader, string id)
        {
            this.Id = id;

            this._loader = loader;
        }

        public void GetContentStream(ObjectLoader.ContentLoader contentLoader)
        {
            this._loader.Load(contentLoader);
        }

        public ITreeEntry Parent { get; set; }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string FullName
        {
            get { throw new NotImplementedException(); }
        }
    }
}
