using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gitty.Storage;

namespace Gitty
{
    public class Tree : TreeEntry
    {
        private readonly ObjectStorage _storage;
        public long Size { get; private set; }

        internal Tree(ObjectStorage storage ,string id, long size, Func<byte[]> loader, Tree parent = null, string name = null, string mode = null)
            : base(ObjectType.Tree, id, parent, name, mode)
        {
            this.Size = size;
            this._storage = storage;
            this._loader = new Lazy<byte[]>(loader);
        }

        private readonly Lazy<byte[]> _loader;
        public byte[] Data
        {
            get { return _loader.Value; }
        }

        private readonly List<TreeEntry> _items = new List<TreeEntry>();
        public virtual IEnumerable<TreeEntry> Items
        {
            get
            {
                this.EnsureLoaded();
                return _items.AsReadOnly();
            }
        }

        private static IEnumerable<T> FlattenTree<T>(IEnumerable<T> entries, Func<T, bool> filter, Func<T, IEnumerable<T>> selector)
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

            return FlattenTree(this.Items, blob => blob.Type == ObjectType.Blob,
                                           tree => ((Tree)tree).EnumerateItems(recursive));
        }

        private bool _loaded;

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            var stream = new MemoryStream(this.Data);

            var bytesRead = 0;

            while (bytesRead < this.Size)
            {
                //read until space for mode
                var mode = stream.ReadUntil(c => c == ' ');
                bytesRead += mode.Length + 1;
                var name = stream.ReadUntil(c => c == '\0');
                bytesRead += name.Length + 1;
                var entryId = stream.ReadId();
                bytesRead += 20;
                var entry = _storage.Read<TreeEntry>(entryId, this, name, mode);

                this._items.Add(entry);
            }

            this._loaded = true;
        }
    }

    class WorkingTreeHelper
    {

        public static bool NotIgnored(Repository repo, FileSystemInfo info)
        {
            //TODO: add support for .gitignore
            return true;
        }

        public static string ModeForFileSystemInfo(FileSystemInfo info)
        {
            throw new NotImplementedException();
        }
    }
}