using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Gitty
{
    public class Tree : ITreeEntry
    {
        private readonly Repository _repository;
        private readonly ObjectLoader _loader;

        public string Id { get; private set; }

        internal Tree(Repository repository, ObjectLoader loader, string id)
        {
            _repository = repository;
            _loader = loader;
            this.Id = id;
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

        private static IEnumerable<T> FlattenTree<T>(IEnumerable<T> entries, Func<T,bool> filter, Func<T, IEnumerable<T>> selector)
        {
            foreach (var entry in entries)
            {
                if (filter(entry))
                {
                    yield return entry;
                }
                else
                {
                    foreach (var subentry in FlattenTree(selector(entry), filter, selector))
                    {
                        yield return subentry;
                    }
                }
            }

        }

        public IEnumerable<TreeEntry> EnumerateItems(bool recursive = false)
        {
            if (!recursive)
                return this.Items;

            return FlattenTree(this.Items, entry => entry.Type == "blob",
                               entry => ((Tree) entry.Entry).EnumerateItems(recursive));
        }

        private bool _loaded;

        public ITreeEntry Parent { get; set; }

        public string Name
        {
            get { return this.Parent.Try(o => o.Name); }
        }

        public string FullName
        {
            get { return this.Parent.Try(o => o.FullName); }
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            this._loader.Load(stream =>
            {
                var bytesRead = 0;
                
                while (bytesRead < _loader.Size)
                {
                    //read until space for mode
                    var mode = stream.ReadUntil(c => c == ' ');
                    bytesRead += mode.Length + 1;
                    var name = stream.ReadUntil(c => c == '\0');
                    bytesRead += name.Length + 1;
                    var id = stream.ReadId();
                    bytesRead += 20;
                    var entry = new TreeEntry(this._repository, id, name, mode) {Parent = this};
                    this._items.Add(entry);
                }
            });

            this._loaded = true;
        }
    }
}