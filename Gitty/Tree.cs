using System.Collections.Generic;

namespace Gitty
{
    public class Tree 
    {
        private readonly Repository _repository;
        private readonly ObjectLoader _loader;

        public string Id { get; private set; }

        internal Tree(Repository repository, ObjectLoader loader)
        {
            _repository = repository;
            _loader = loader;
            this.Id = loader.Id;
        }

        private readonly List<TreeEntry> _items = new List<TreeEntry>();
        public IEnumerable<TreeEntry> Items
        {
            get
            {
                this.EnsureLoaded();
                return _items.AsReadOnly();
            }
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            this._loader.Load((stream, info) =>
            {
                var bytesRead = 0;
                
                while (bytesRead < info.Size)
                {
                    //read until space for mode
                    var mode = stream.ReadUntil(c => c == ' ');
                    bytesRead += mode.Length + 1;
                    var name = stream.ReadUntil(c => c == '\0');
                    bytesRead += name.Length + 1;
                    var id = stream.ReadId();
                    bytesRead += 20;
                    this._items.Add(new TreeEntry(this._repository, id, name, mode));
                }
            });

            this._loaded = true;
        }
    }
}