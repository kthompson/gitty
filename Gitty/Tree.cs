using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gitty.Storage;

namespace Gitty
{
    /// <summary>
    /// Represents the git object Tree that is used to represent folders
    /// </summary>
    public class Tree : TreeEntry
    {
        #region Private Variables 

        private string _id;

        private readonly List<TreeEntry> _items = new List<TreeEntry>();

        private bool _loaded;
        private readonly Lazy<byte[]> _loader;
        private readonly ObjectStorage _storage;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data
        {
            get { return _loader.Value; }
        }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The SHA1 id of the object.
        /// </value>
        public override string Id
        {
            get { return base.Id ?? _id ?? (_id = ObjectWriter.ComputeId(this)); }
        }
        /// <summary>
        /// Gets the items in the tree.
        /// </summary>
        public virtual IEnumerable<TreeEntry> Items
        {
            get
            {
                this.EnsureLoaded();
                return _items.AsReadOnly();
            }
        }
        /// <summary>
        /// Gets the size.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets the ObjectType.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Tree; }
        }
        #endregion

        #region Constructors
        internal Tree(ObjectStorage storage ,string id, long size, Func<byte[]> loader, Tree parent = null, string name = null, string mode = null)
            : base(id, parent, name, mode)
        {
            this.Size = size;
            this._storage = storage;
            this._loader = loader.Try(n => new Lazy<byte[]>(loader));
        }
        #endregion         

        #region Private Methods

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            using (var stream = new MemoryStream(this.Data))
            {

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
            }

            this._loaded = true;
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Enumerates the items in the tree.
        /// </summary>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public IEnumerable<TreeEntry> EnumerateItems(bool recursive = false)
        {
            if (!recursive)
                return this.Items;

            return RecursiveTreeEnumerator(this);
        }
        #endregion
    }
}