using System.Linq;
using System.Security.AccessControl;

namespace Gitty
{
    public class Blob 
    {
        private readonly Repository _repository;
        private readonly ObjectLoader _loader;

        public string Id { get; private set; }

        internal Blob(Repository repository, ObjectLoader loader)
        {
            this.Id = loader.Id;

            this._repository = repository;
            this._loader = loader;
        }

        public void GetContentStream(ObjectLoader.ContentLoader contentLoader)
        {
            this._loader.Load(contentLoader);
        }
    }
}
