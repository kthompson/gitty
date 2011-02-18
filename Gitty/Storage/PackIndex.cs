using System;
using System.Collections.Generic;
using System.IO;

namespace Gitty.Storage
{
    /// <summary>
    /// A <see cref="PackFile"/> Index for fast lookup of objects
    /// </summary>
    public class PackIndex
    {
        /// <summary>
        /// Gets the location.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Gets the fanout table offset.
        /// </summary>
        public int FanoutTableOffset { get; private set; }
        /// <summary>
        /// Gets the sha table offset.
        /// </summary>
        public int ShaTableOffset { get; private set; }
        /// <summary>
        /// Gets the CRC table offset.
        /// </summary>
        public int CrcTableOffset { get; private set; }
        /// <summary>
        /// Gets the offset table offset.
        /// </summary>
        public int OffsetTableOffset {get; private set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="PackIndex"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="size">The size.</param>
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
        
        /// <summary>
        /// Gets the PackIndexEntry from the index.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public PackIndexEntry GetEntry(string id)
        {
            //TODO: we need to support 64bit offsets too
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

        /// <summary>
        /// Determines whether the specified id exists in the index.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        ///   <c>true</c> if the specified id has entry; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Represent an entry in a <see cref="PackIndex"/>.
        /// </summary>
        public class PackIndexEntry
        {
            /// <summary>
            /// Gets the id.
            /// </summary>
            public string Id { get; private set; }
            /// <summary>
            /// Gets the index.
            /// </summary>
            public int Index { get; private set; }
            /// <summary>
            /// Gets the CRC.
            /// </summary>
            public int Crc { get; private set; }
            /// <summary>
            /// Gets the offset.
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PackIndexEntry"/> class.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="index">The index.</param>
            /// <param name="crc">The CRC.</param>
            /// <param name="offset">The offset.</param>
            internal PackIndexEntry(string id, int index, int crc, int offset)
            {
                this.Id = id;
                this.Index = index;
                this.Crc = crc;
                this.Offset = offset;
            }
        }
    }
}