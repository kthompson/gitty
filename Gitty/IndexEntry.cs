using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    public class IndexEntry
    {
        public IndexEntry(BinaryReader reader)
        {
            var position = reader.BaseStream.Position;
            this.CreationTime = reader.ReadBigEndianInt32() * 1000000000L + (reader.ReadBigEndianInt32() % 1000000000L);
            this.ModifiedTime = reader.ReadBigEndianInt32() * 1000000000L + (reader.ReadBigEndianInt32() % 1000000000L);

            this.Dev = reader.ReadBigEndianInt32();
            this.Ino = reader.ReadBigEndianInt32();
            this.Mode = reader.ReadBigEndianInt32();
            this.Uid = reader.ReadBigEndianInt32();
            this.Gid = reader.ReadBigEndianInt32();
            this.Size = reader.ReadBigEndianInt32();

            var sha = reader.ReadBytes(20);
            this.Id = Helper.ByteArrayToId(sha);

            this.Flags = reader.ReadBigEndianInt16();
            //stages = (1 << getStage());
            this.Name = Encoding.ASCII.GetString(reader.ReadBytes(this.Flags & 0xfff));
            reader.BaseStream.Position = position + ((70 + this.Name.Length) & ~7);
        }

        public string Name { get; private set; }
        public string Id { get; private set; }
        public short Flags { get; private set; }
        public int Size { get; private set; }
        public int Gid{ get; private set; }
        public int Uid { get; private set; }
        public int Mode { get; private set; }
        public int Ino { get; private set; }
        public int Dev { get; private set; }
        public long CreationTime { get; private set; }
        public long ModifiedTime { get; private set; }
    }
}
