using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Security
{
    public class OverAESStream : EncryptedStream
    {
        public OverAESStream(Stream stream, byte[] key, byte[] iv, int nbIterations)
             : base(stream)
        {
            this.NbIterations = nbIterations;
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
        
        private int NbIterations
        {
            get;
            set;
        }
        
        protected override byte[] Encrypt(byte[] data, int start, int length)
        {
            return OverAES.Encrypt(data, start, length, Key, IV, NbIterations);
        }
        protected override byte[] Decrypt(byte[] data, int start, int length)
        {
            return OverAES.Decrypt(data, start, length, Key, IV, NbIterations);
        }
    }
}