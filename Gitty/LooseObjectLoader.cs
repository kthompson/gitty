using System.IO;

namespace Gitty
{
    class LooseObjectLoader : ObjectLoader
    {
        public string Location { get; private set; }

        internal LooseObjectLoader(Repository repository, string id)
            : base(repository, id)
        {
            this.Location = Path.Combine(repository.Location, "objects", id.Substring(0, 2), id.Substring(2));
        }

        public override bool Exists
        {
            get { return File.Exists(this.Location); }
        }

        protected override Stream OpenStream()
        {
            return new FileStream(this.Location, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}