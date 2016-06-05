using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Security
{
    public class AESStream : EncryptedStream
    {
        public AESStream(Stream stream, byte[] key, byte[] iv)
             : base(stream)
        {
            this.Key = key;
            this.IV = iv;
        }
        
        private byte[] Key
        {
            get;
            set;
        }
        
        private byte[] IV
        {
            get;
            set;
        }
        
        protected override byte[] Encrypt(byte[] data, int start, int length)
        {
            return AES.Encrypt(data, start, length, Key, IV);
        }
        protected override byte[] Decrypt(byte[] data, int start, int length)
        {
            return AES.Decrypt(data, start, length, Key, IV);
        }
    }
}