using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// A class that represents the Index of objects to be added to the repository.
    /// </summary>
    public class Index
    {
        private int _entryCount;
        /// <summary>
        /// Gets the number of entries.
        /// </summary>
        public int EntryCount
        {
            get
            {
                this.EnsureLoaded();
                return _entryCount;
            }
        }

        private int _version;
        /// <summary>
        /// Gets the index version.
        /// </summary>
        public int Version
        {
            get
            {
                this.EnsureLoaded();
                return _version;
            }
        }

        /// <summary>
        /// Gets the location of .git/index.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public Index(string location)
        {
            this.Location = location;
        }

        private readonly List<IndexEntry> _entries = new List<IndexEntry>();
        /// <summary>
        /// Gets the entries.
        /// </summary>
        public IEnumerable<IndexEntry> Entries
        {
            get
            {
                this.EnsureLoaded();
                return _entries.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has unmerged paths.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has unmerged paths; otherwise, <c>false</c>.
        /// </value>
        public bool HasUnmergedPaths
        {
            get { throw new NotImplementedException(); }
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if(_loaded)
                return;

            using (var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                var sig = reader.ReadBytes(4);

                if (!(sig[0] == 'D' &&
                      sig[1] == 'I' &&
                      sig[2] == 'R' &&
                      sig[3] == 'C'))
                {
                    throw new InvalidOperationException("not a index file");
                }

                this._version = reader.ReadBigEndianInt32();
                this._entryCount = reader.ReadBigEndianInt32();
                var count = this._entryCount;
                while (count-- > 0)
                    this._entries.Add(new IndexEntry(reader));
            }

            _loaded = true;
        }
    }
}
