using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Represents an Entry in the GIT index.
    /// </summary>
    public class IndexEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexEntry"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
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

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the flags.
        /// </summary>
        public short Flags { get; private set; }
        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Gets the gid.
        /// </summary>
        public int Gid{ get; private set; }
        /// <summary>
        /// Gets the uid.
        /// </summary>
        public int Uid { get; private set; }
        /// <summary>
        /// Gets the mode.
        /// </summary>
        public int Mode { get; private set; }
        /// <summary>
        /// Gets the ino.
        /// </summary>
        public int Ino { get; private set; }
        /// <summary>
        /// Gets the dev.
        /// </summary>
        public int Dev { get; private set; }
        /// <summary>
        /// Gets the creation time.
        /// </summary>
        public long CreationTime { get; private set; }
        /// <summary>
        /// Gets the modified time.
        /// </summary>
        public long ModifiedTime { get; private set; }
    }
}
