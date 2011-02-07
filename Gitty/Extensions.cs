using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    public static class Extensions
    {
        public static long Read7BitEncodedInt(this Stream file, long seed = 0, int bits = 0)
        {

            long result = seed;

            if (bits > 0)
                result <<= bits;

            int c = file.ReadByte() & 0xff;
            result += c & 127;

            while ((c & 128) != 0)
            {
                result += 1;
                c = file.ReadByte() & 0xff;
                result <<= 7;
                result += (c & 127);
            }

            return result;
        }

        public static TResult Try<T, TResult>(this T obj, Func<T, TResult> tryMethod)
            where T : class
            where TResult : class
        {
            return obj == null ? null : tryMethod(obj);
        }

        public static int ReadBigEndianInt32(this BinaryReader reader)
        {
            return IPAddress.HostToNetworkOrder(reader.ReadInt32());
        }

        public static short ReadBigEndianInt16(this BinaryReader reader)
        {
            return IPAddress.HostToNetworkOrder(reader.ReadInt16());
        }

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
