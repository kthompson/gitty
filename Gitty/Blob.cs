using System.Linq;
using System.Security.AccessControl;

namespace Gitty
{
    public class Blob 
    {
        private readonly ObjectLoader _loader;

        public string Id { get; private set; }

        internal Blob(ObjectLoader loader)
        {
            this.Id = loader.Id;

            this._loader = loader;
        }

        public void GetContentStream(ObjectLoader.ContentLoader contentLoader)
        {
            this._loader.Load(contentLoader);
        }
    }
}
