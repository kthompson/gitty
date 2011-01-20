namespace Gitty
{
    public class TreeEntry
    {
        private readonly Repository _repository;

        public string Id { get; private set; }
        public string Mode { get; private set; }
        public string Name { get; private set; }

        public string Type { get; private set; }

        public object Entry { get; private set; }

        internal TreeEntry(Repository repository, string id, string name, string mode)
        {
            this._repository = repository;

            this.Id = id;
            this.Name = name;
            this.Mode = mode;
            this.Entry = this._repository.OpenObject(id);
            this.Type = this.Entry is Tree ? "tree" : "blob";
        }
    }
}