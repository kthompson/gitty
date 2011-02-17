using System;
using System.IO;
using System.Text;

namespace Gitty.Storage
{
    class PackFile
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

        private PackIndex _index;
        public PackIndex Index
        {
            get
            {
                this.EnsureLoaded();
                return _index;
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
        public string IndexLocation { get; private set; }

        public PackFile(string packFile)
        {
            this.Location = Helper.MakeAbsolutePath(packFile);
            if (this.Location.EndsWith(".pack"))
                this.IndexLocation = this.Location.Substring(0, this.Location.Length - 4) + "idx";
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded)
                return;
            
            using (var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                var sig = reader.ReadBytes(4);

                if (!(sig[0] == 'P' &&
                      sig[1] == 'A' &&
                      sig[2] == 'C' &&
                      sig[3] == 'K'))
                {
                    throw new InvalidOperationException("not a pack file");
                }

                this._version = reader.ReadBigEndianInt32();
                this._entryCount = reader.ReadBigEndianInt32();
                this._index = new PackIndex(this.IndexLocation, this._entryCount);
            }
            this._loaded = true;
        }

        public bool HasEntry(string id)
        {
            return this.Index.HasEntry(id);
        }

        public PackedObjectReader GetObjectLoader(string id)
        {
            var info = this.Index.GetEntry(id);
            return GetObjectLoader(info.Offset);
        }

        public PackedObjectReader GetObjectLoader(long offset)
        {
            return PackedObjectReader.Create(this, offset);
        }
    }
}