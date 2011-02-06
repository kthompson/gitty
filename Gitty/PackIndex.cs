using System;
using System.IO;
using System.Net;

namespace Gitty
{
    public class PackIndex
    {
        public string Location { get; private set; }

        private int _size;
        private int _fanoutStartOffset;
        private int _sha1StartOffset;
        private int _crcStartOffset;
        private int _offsetStartOffset;

        public PackIndex(string location, int size)
        {
            this.Location = location;
            _size = size;
            _fanoutStartOffset = 8;

            _sha1StartOffset = _fanoutStartOffset + 256 * 4;
            _crcStartOffset = _sha1StartOffset + 20*_size; // sha records
            _offsetStartOffset = _crcStartOffset + 4 * _size; // sha records
        }


        //TODO: we need to support 64bit offsets too
        public PackIndexEntry GetEntry(string id)
        {
            var idBytes = Helper.IdToByteArray(id);
            var fanoutIndex = idBytes[0];
            using(var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                //seek to fanout index
                reader.BaseStream.Seek(_fanoutStartOffset + fanoutIndex * 4, SeekOrigin.Begin);
                var index = reader.ReadBigEndianInt32();

                reader.BaseStream.Seek(_sha1StartOffset + index*20, SeekOrigin.Begin);
                // TODO do a binary search to find the Id
                
                while(true)
                {
                    var position = reader.BaseStream.Position;
                    if (position < _sha1StartOffset)
                        throw new InvalidOperationException("tried to read before the sha table");

                    //TODO do byte-by-byte compare
                    if(reader.BaseStream.ReadId() == id)
                    {
                        break;
                    }

                    index--;
                    reader.BaseStream.Position = position - 20;
                }

                reader.BaseStream.Seek(_crcStartOffset + index * 4, SeekOrigin.Begin);
                var crc = reader.ReadBigEndianInt32();

                reader.BaseStream.Seek(_offsetStartOffset + index * 4, SeekOrigin.Begin);
                var offset = reader.ReadBigEndianInt32();

                return new PackIndexEntry(id, crc, offset);
            }
        }

        public class PackIndexEntry
        {
            public string Id { get; private set; }
            public int Crc { get; private set; }
            public int Offset { get; private set; }

            public PackIndexEntry(string id, int crc, int offset)
            {
                this.Id = id;
                this.Crc = crc;
                this.Offset = offset;
            }
        }
    }
}