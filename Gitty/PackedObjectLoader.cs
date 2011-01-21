using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;

namespace Gitty
{
    class PackedObjectLoader : ObjectLoader
    {
        public PackFile PackFile { get; private set; }

        public PackedObjectLoader(PackFile packFile, string id)
            : base(id)
        {
            this.PackFile = packFile;
        }

        public override ObjectLoadInfo Load(ContentLoader contentLoader = null)
        {
            var entry = this.PackFile.Index.GetEntry(this.Id);

            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(entry.Offset, SeekOrigin.Begin);
                
                var info = GetObjectInfo(file);

                if (contentLoader != null)
                {
                    using (
                        var stream = new CompressionStream(file, System.IO.Compression.CompressionMode.Compress, true))
                    {
                        contentLoader(stream, info);
                    }
                }

                return info;
            }
        }

        private static ObjectLoadInfo GetObjectInfo(Stream file)
        {
            var b = file.ReadByte();
            var type = (b & 0x70) >> 4;

            var size = GetSize(file, b);

            var typeString = GetTypeString(type);

            return new ObjectLoadInfo(typeString, size);
        }

        private static int GetSize(Stream file, int b)
        {
            var size = (b & 0xF);
            var sizeBits = 4;
            while (true)
            {
                if ((b & 0x80) != 0x80)
                    break;

                b = file.ReadByte();
                size |= (b & 0x7F) << sizeBits;
                sizeBits += 7;
            }
            return size;
        }

        private static string GetTypeString(int type)
        {
            switch (type)
            {
                case 1:
                    return "commit";
                case 2:
                    return "tree";
                case 3:
                    return "blob";
                case 4:
                    return "tag";
                case 6: 
                    return "ofs_delta";
                case 7:
                    return "ref_delta";
                default:
                    throw new InvalidOperationException(string.Format("invalid type: {0}", type));
            }
        }
    }

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

        private void EnsureLoaded()
        {
            using (var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                var sig = reader.ReadBytes(4);

                if (!(sig[0] == 'P' &&
                      sig[1] == 'A' &&
                      sig[1] == 'C' &&
                      sig[1] == 'K'))
                {
                    throw new InvalidOperationException("not a pack file");
                }

                this._version = reader.ReadInt32();
                this._entryCount = reader.ReadInt32();
                this._index = new PackIndex(this.IndexLocation, this.EntryCount);
            }
        }

        internal static IEnumerable<PackFile> FindAll(Repository repository)
        {
            var packs = new DirectoryInfo(repository.PacksLocation);
            if(packs.Exists)
                return packs
                      .EnumerateFiles("*.pack")
                      .Select(pf => new PackFile(pf.FullName));

            return new PackFile[] {};
        }

        public bool HasEntry(string id)
        {
            return this.Index.GetEntry(id) != null;
        }

        public ObjectLoader GetObjectLoader(string id)
        {
            return new PackedObjectLoader(this, id);
        }
    }

    //V2 PackIndex
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
                var index = IPAddress.NetworkToHostOrder(reader.ReadInt32());

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
                var crc = IPAddress.NetworkToHostOrder(reader.ReadInt32());

                reader.BaseStream.Seek(_offsetStartOffset + index * 4, SeekOrigin.Begin);
                var offset = IPAddress.NetworkToHostOrder(reader.ReadInt32());

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