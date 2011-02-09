using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Gitty
{
    internal class CompressionStream : DeflateStream
    {
        private static Stream MoveStream(Stream stream)
        {
            // HACK: we need this to get the DeflateStream to read properly
            stream.ReadByte();
            stream.ReadByte();
            return stream;
        }

        private static FileStream OpenFile(string fileLocation)
        {
            return new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public CompressionStream(string fileLocation, CompressionMode mode = CompressionMode.Decompress, bool leaveOpen = false)
            : this(OpenFile(fileLocation), mode, leaveOpen)
        {
            
        }

        public CompressionStream(Stream stream, CompressionMode mode = CompressionMode.Decompress, bool leaveOpen = false)
            : base(MoveStream(stream), mode, leaveOpen)
        {
        }
    }
}
