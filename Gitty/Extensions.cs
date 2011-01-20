using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    public static class Extensions
    {
        public static string ReadUntil(this Stream stream, Predicate<int> predicate)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var c = stream.ReadByte();
                if (predicate(c) || c == -1)
                    return sb.ToString();

                sb.Append((char) c);
            }
        }

        public static string ReadWhile(this Stream stream, Predicate<int> predicate)
        {
            return ReadUntil(stream, c => !predicate(c));
        }

        public static string ReadId(this Stream stream)
        {
            var count = 0;
            var sb = new StringBuilder();
            while (count++ < 20)
            {
                var c = stream.ReadByte();
                if(c == -1)
                    throw new InvalidOperationException("Not enough bytes to read Id");

                var hex = c.ToString("x").PadLeft(2, '0');

                sb.Append(hex);
            }

            return sb.ToString();
        }
    }
}
