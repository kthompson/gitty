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

        private static IEnumerable<TreeEntry> RecursiveTreeEnumerator(Tree tree)
        {
            var stack = new Stack<IEnumerator<TreeEntry>>();
            stack.Push(tree.Items.GetEnumerator());

            do
            {
                var enumerator = stack.Pop();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Type == ObjectType.Tree)
                    {
                        stack.Push(enumerator);
                        enumerator = ((Tree) enumerator.Current).Items.GetEnumerator();
                    }
                    else
                    {
                        yield return enumerator.Current;
                    }
                }
                
            } while (stack.Count > 0);
        }

        public IEnumerable<TreeEntry> EnumerateItems(bool recursive = false)
        {
            if (!recursive)
                return this.Items;

            return RecursiveTreeEnumerator(this);
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