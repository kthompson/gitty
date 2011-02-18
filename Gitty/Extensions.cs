using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Extensions for miscellaneous things
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Read a 7bit encoded int from a stream.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="seed">The seed.</param>
        /// <param name="bits">The bits.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Tries an action on an object and returns it unless the object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="tryMethod">The try method.</param>
        /// <returns>null when the obj is null or TResult</returns>
        public static TResult Try<T, TResult>(this T obj, Func<T, TResult> tryMethod)
            where T : class
            where TResult : class
        {
            return obj == null ? null : tryMethod(obj);
        }

        /// <summary>
        /// Reads a big endian int from a BinaryReader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static int ReadBigEndianInt32(this BinaryReader reader)
        {
            return IPAddress.HostToNetworkOrder(reader.ReadInt32());
        }

        /// <summary>
        /// Reads a big endian short from a BinaryReader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static short ReadBigEndianInt16(this BinaryReader reader)
        {
            return IPAddress.HostToNetworkOrder(reader.ReadInt16());
        }

        /// <summary>
        /// Reads from the stream until the predicate is satisfied or EndOfStream is reached.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static string ReadUntil(this Stream stream, Predicate<int> predicate)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var c = stream.ReadByte();
                if (predicate(c) || c == -1)
                    return sb.ToString();

                sb.Append((char)c);
            }
        }

        /// <summary>
        /// Skips bytes in the stream until the predicate is satisfied or EndOfStream is reached.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="predicate">The predicate.</param>
        public static void SkipUntil(this Stream stream, Predicate<int> predicate)
        {
            while (true)
            {
                var c = stream.ReadByte();
                if (predicate(c) || c == -1)
                    return ;
            }
        }

        /// <summary>
        /// Reads a SHA1 id from a stream and converts it to a string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
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
