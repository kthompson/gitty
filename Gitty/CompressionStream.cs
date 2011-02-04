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

        public CompressionStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(MoveStream(stream), mode, leaveOpen)
        {
        }


        public CompressionStream(Stream stream, CompressionMode mode)
            : base(MoveStream(stream), mode)
        {
        }
    }
}
