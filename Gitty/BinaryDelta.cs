/*
 * Copyright (C) 2007, Robin Rosenberg <robin.rosenberg@dewire.com>
 * Copyright (C) 2007, Shawn O. Pearce <spearce@spearce.org>
 * Copyright (C) 2011, Kevin Thompson <kevin.thompson@theautomaters.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace Gitty
{
    /// <summary>
    /// Recreate a stream from a base stream and a GIT pack delta.
    /// <para />
    /// This entire class is heavily cribbed from <code>patch-delta.c</code> in the
    /// GIT project. The original delta patching code was written by Nicolas Pitre
    /// (<nico@cam.org>).
    /// </summary>
    public static class BinaryDelta
    {
        ///	<summary>
        /// Apply the changes defined by delta to the data in base, yielding a new
        /// array of bytes.
        /// </summary>
        /// <param name="baseData">some byte representing an object of some kind.</param>
        ///	<param name="delta">
        /// A git pack delta defining the transform from one version to
        /// another.
        /// </param>
        ///	<returns>Patched base</returns>
        public static byte[] Apply(byte[] baseData, byte[] delta)
        {
            var deltaPtr = 0;

            var baseLen = Get7BitEncodedInt(delta, ref deltaPtr);
            if (baseData.Length != baseLen)
                throw new ArgumentException("baseData Length incorrect");

            var resLen = Get7BitEncodedInt(delta, ref deltaPtr);
            

            var result = new byte[resLen];
            var resultPtr = 0;
            while (deltaPtr < delta.Length)
            {
                var cmd = delta[deltaPtr++] & 0xff;
                if ((cmd & 0x80) != 0)
                {
                    // Determine the segment of the base which should
                    // be copied into the output. The segment is given
                    // as an offset and a Length.
                    //
                    int copyOffset;
                    int copySize;
                    deltaPtr = GetCommandParameters(delta, deltaPtr, cmd, out copyOffset, out copySize);

                    Array.Copy(baseData, copyOffset, result, resultPtr, copySize);
                    resultPtr += copySize;
                }
                else if (cmd != 0)
                {
                    // Anything else the data is literal within the delta
                    // itself.
                    //
                    Array.Copy(delta, deltaPtr, result, resultPtr, cmd);
                    deltaPtr += cmd;
                    resultPtr += cmd;
                }
                else
                {
                    // cmd == 0 has been reserved for future encoding but
                    // for now its not acceptable.
                    //
                    throw new ArgumentException("unsupported command 0");
                }
            }

            return result;
        }

        private static int GetCommandParameters(byte[] delta, int offset, int cmd, out int copyOffset, out int copySize)
        {
            copyOffset = 0;
            if ((cmd & 0x01) != 0)
                copyOffset = delta[offset++] & 0xff;

            if ((cmd & 0x02) != 0)
                copyOffset |= (delta[offset++] & 0xff) << 8;

            if ((cmd & 0x04) != 0)
                copyOffset |= (delta[offset++] & 0xff) << 16;

            if ((cmd & 0x08) != 0)
                copyOffset |= (delta[offset++] & 0xff) << 24;

            copySize = 0;
            if ((cmd & 0x10) != 0)
                copySize = delta[offset++] & 0xff;

            if ((cmd & 0x20) != 0)
                copySize |= (delta[offset++] & 0xff) << 8;

            if ((cmd & 0x40) != 0)
                copySize |= (delta[offset++] & 0xff) << 16;

            if (copySize == 0)
                copySize = 0x10000;

            return offset;
        }

        private static int Get7BitEncodedInt(byte[] delta, ref int offset)
        {
            var shift = 0;
            int c;
            var encodedInt = 0;
            do
            {
                c = delta[offset++] & 0xff;
                encodedInt |= (c & 0x7f) << shift;
                shift += 7;
            } while ((c & 0x80) != 0);

            return encodedInt;
        }
    }
}


