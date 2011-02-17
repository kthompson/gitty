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
    public class MessageDigest : IDisposable
    {
        private CryptoStream _stream;
        private SHA1Managed _hash;

        public MessageDigest()
        {
            Init();
        }

        private void Init()
        {
            _hash = new SHA1Managed();
            _stream = new CryptoStream(Stream.Null, _hash, CryptoStreamMode.Write);
        }

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

        public static byte[] Digest(byte[] input)
        {
            using (var me = new MessageDigest())
            {
                me.Update(input);
                return me.Digest();
            }
        }

        public void Reset()
        {
            Dispose();
            Init();
        }

        public void Update(byte input)
        {
            _stream.WriteByte(input);
        }

        public void Update(byte[] input)
        {
            this.Update(input, 0, input.Length);
        }

        public void Update(byte[] input, int index, int count)
        {
            _stream.Write(input, index, count);
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();
            _stream = null;
        }

        public void Update(Stream stream)
        {
            stream.CopyTo(_stream);
        }
    }
}
