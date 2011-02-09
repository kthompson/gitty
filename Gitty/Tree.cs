using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Gitty
{
    public class Tree : TreeEntry
    {
        internal Tree(Repository repository, ObjectLoader loader, string id, string name = null, string mode = null, Tree parent = null)
            : base(repository, loader, id, name, mode, parent)
        {
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

            return FlattenTree(this.Items, entry => entry.Type == ObjectType.Blob,
                                           entry => ((Tree) entry).EnumerateItems(recursive));
        }

        private bool _loaded;

        public override ObjectType Type
        {
            get { return ObjectType.Tree; }
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            this.Loader.Load(stream =>
            {
                var bytesRead = 0;
                
                while (bytesRead < Loader.Size)
                {
                    //read until space for mode
                    var mode = stream.ReadUntil(c => c == ' ');
                    bytesRead += mode.Length + 1;
                    var name = stream.ReadUntil(c => c == '\0');
                    bytesRead += name.Length + 1;
                    var id = stream.ReadId();
                    bytesRead += 20;
                    var entry = this.Repository.OpenTreeEntry(id, name, mode, this);
                    this._items.Add(entry);
                }
            });

            this._loaded = true;
        }
    }
}