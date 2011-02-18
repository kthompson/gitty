/*
 * Copyright (C) 2010, Kevin Thompson <kevin.thompson@theautomaters.com>
 * Copyright (C) 2009, Henon <meinrad.recheis@gmail.com>
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
using System.Security.Cryptography;
using System.IO;

namespace Gitty
{
    /// <summary>
    /// MessageDigest class used to calculate SHA1 ids.
    /// </summary>
    public class MessageDigest : IDisposable
    {
        private CryptoStream _stream;
        private SHA1Managed _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDigest"/> class.
        /// </summary>
        public MessageDigest()
        {
            Init();
        }

        private void Init()
        {
            _hash = new SHA1Managed();
            _stream = new CryptoStream(Stream.Null, _hash, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Gets the result of the calculation.
        /// </summary>
        /// <returns></returns>
        public byte[] Digest()
        {
            try
            {
                _stream.FlushFinalBlock();
                return _hash.Hash;    
            }
            finally
            {
                Reset();    
            }
        }

        /// <summary>
        /// Gets the result of a calculation that has a simple input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static byte[] Digest(byte[] input)
        {
            using (var me = new MessageDigest())
            {
                me.Update(input);
                return me.Digest();
            }
        }

        /// <summary>
        /// Resets this instance for reuse.
        /// </summary>
        public void Reset()
        {
            Dispose();
            Init();
        }

        /// <summary>
        /// Updates the calculation with the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Update(byte input)
        {
            _stream.WriteByte(input);
        }

        /// <summary>
        /// Updates the calculation with the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Update(byte[] input)
        {
            this.Update(input, 0, input.Length);
        }

        /// <summary>
        /// Updates the calculation with the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public void Update(byte[] input, int index, int count)
        {
            _stream.Write(input, index, count);
        }

        /// <summary>
        /// Updates the calculation with the specified input.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Update(Stream stream)
        {
            stream.CopyTo(_stream);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(!disposing)
                return;
            
            if (_stream != null)
                _stream.Dispose();

            _stream = null;

            if(_hash != null)
                _hash.Dispose();

            _hash = null;

           
        }
    }
}
