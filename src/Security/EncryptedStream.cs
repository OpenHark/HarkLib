using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Security
{
    public abstract class EncryptedStream : HarkLib.Core.BasedStream
    {
        public EncryptedStream(Stream stream)
             : base(stream)
        { }
        
        private byte[] buffer = null;
        private int bufferIndex = 0;
        
        protected abstract byte[] Encrypt(byte[] data, int start, int length);
        protected abstract byte[] Decrypt(byte[] data, int start, int length);
        
        public override int Read(byte[] data, int start, int length)
        {
            return Read(data, start, length, null);
        }
        
        public int Read(byte[] data, int start, int length, byte[] idBytes)
        {
            if(buffer != null)
            {
                int bufferSize = buffer.Length - bufferIndex;
                int sizeToExtract = Math.Min(length, bufferSize);
                
                Array.Copy(buffer, bufferIndex, data, start, sizeToExtract);
                
                if(bufferSize == length)
                {
                    buffer = null;
                    return length;
                }
                
                if(bufferSize < length)
                {
                    buffer = null;
                    return sizeToExtract;
                }
                
                bufferIndex += length;
                return length;
            }
            
            if(idBytes != null)
            {
                byte[] rd = BaseStream.ReadWrapped();
                if(rd.Length == 0)
                    return 0;
                
                byte[] dd = Decrypt(rd, 0, rd.Length);
                
                if(dd.Length != idBytes.Length)
                    throw new IOException("Wrong ID detected.");
                
                for(int i = 0; i < idBytes.Length; ++i)
                    if(idBytes[i] != dd[i])
                        throw new IOException("Wrong ID detected.");
            }
            
            byte[] receivedData = BaseStream.ReadWrapped();
            if(receivedData.Length == 0)
                return 0;
            
            byte[] decryptedData = Decrypt(receivedData, 0, receivedData.Length);
            int dlength = decryptedData.Length;
            
            if(length > dlength)
            {
                Array.Copy(decryptedData, 0, data, start, dlength);
                return dlength;
            }
            else
            {
                Array.Copy(decryptedData, 0, data, start, length);
                
                if(dlength > length)
                {
                    buffer = decryptedData;
                    bufferIndex = length;
                }
                
                return length;
            }
        }
        
        public override void Write(byte[] data, int start, int length)
        {
            if(length == 0)
            {
                BaseStream.WriteWrapped(new byte[0]);
                return;
            }
            
            byte[] dataToSend = Encrypt(data, start, length);
            BaseStream.WriteWrapped(dataToSend);
        }
    }
}