using System;
using System.Collections.Generic;
using System.IO;

namespace Gitty.Storage
{
    public class PackIndex
    {
        public string Location { get; private set; }

        public int Size { get; private set; }
        public int FanoutTableOffset { get; private set; }
        public int ShaTableOffset { get; private set; }
        public int CrcTableOffset { get; private set; }
        public int OffsetTableOffset {get; private set;}

        public PackIndex(string location, int size)
        {
            this.Location = location;
            this.Size = size;

            this.FanoutTableOffset = 8;

            this.ShaTableOffset = FanoutTableOffset + (256 * 4);
            this.CrcTableOffset = ShaTableOffset + (20 * Size); // sha records
            this.OffsetTableOffset = CrcTableOffset + (4 * Size); // sha records
        }

        private readonly Dictionary<string, PackIndexEntry> _entries = new Dictionary<string, PackIndexEntry>();

        private bool _loaded;
        private void EnsureLoaded()
        {
            lock (this)
            {
                if (_loaded)
                    return;

                using (var stream = File.OpenRead(this.Location))
                {
                    stream.Seek(ShaTableOffset, SeekOrigin.Begin);
                    for (int i = 0; i < Size; i++)
                    {
                        var id = stream.ReadId();
                        var entry = new PackIndexEntry(id, i, -1, -1);
                        _entries.Add(id, entry);
                    }
                }

                _loaded = true;
            }
        }

        //TODO: we need to support 64bit offsets too
        public PackIndexEntry GetEntry(string id)
        {
            this.EnsureLoaded();

            using(var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                PackIndexEntry entry;
                if(_entries.TryGetValue(id, out entry))
                {
                    reader.BaseStream.Seek(CrcOffset(entry), SeekOrigin.Begin);
                    var crc = reader.ReadBigEndianInt32();

                    reader.BaseStream.Seek(OffsetOffset(entry), SeekOrigin.Begin);
                    var offset = reader.ReadBigEndianInt32();

                    return new PackIndexEntry(entry.Id, entry.Index, crc, offset);
                }
            }

            return null;
        }

        private int CrcOffset(PackIndexEntry entry)
        {
            return CrcTableOffset + entry.Index * 4;
        }

        private int OffsetOffset(PackIndexEntry entry)
        {
            return OffsetTableOffset + entry.Index * 4;
        }

        public bool HasEntry(string id)
        {
            this.EnsureLoaded();
            return _entries.ContainsKey(id);
        }

        private int GetShaTableEntryOffset(int index)
        {
            return ShaTableOffset + index*20;
        }

        private int GetFanoutTablePosition(byte fanoutIndex)
        {
            return FanoutTableOffset + fanoutIndex * 4;
        }

        public class PackIndexEntry
        {
            public string Id { get; private set; }
            public int Index { get; private set; }
            public int Crc { get; private set; }
            public int Offset { get; private set; }

            public PackIndexEntry(string id, int index, int crc, int offset)
            {
                this.Id = id;
                this.Index = index;
                this.Crc = crc;
                this.Offset = offset;
            }
        }
    }
}