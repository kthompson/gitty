using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    public class Index
    {
        private int _entryCount;
        public int EntryCount
        {
            get
            {
                this.EnsureLoaded();
                return _entryCount;
            }
        }

        private int _version;
        public int Version
        {
            get
            {
                this.EnsureLoaded();
                return _version;
            }
        }

        public string Location { get; private set; }

        public Index(string location)
        {
            this.Location = location;
        }

        private readonly List<IndexEntry> _entries = new List<IndexEntry>();
        public IEnumerable<IndexEntry> Entries
        {
            get
            {
                this.EnsureLoaded();
                return _entries.AsReadOnly();
            }
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

                this._version = reader.ReadBigInt32();
                this._entryCount = reader.ReadBigInt32();
                var count = this._entryCount;
                while (count-- > 0)
                    this._entries.Add(new IndexEntry(reader));
            }

            _loaded = true;
        }
    }
}
