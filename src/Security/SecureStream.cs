using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

using HarkLib.Core;

namespace HarkLib.Security
{
    public class SecureStream : BasedStream
    {
        public SecureStream(Stream baseStream, int nbIterations = 1000)
            : base(baseStream)
        {
            this.NbIterations = nbIterations;
            
            Negotiate();
        }
        
        public int NbIterations
        {
            get;
            private set;
        }
        
        public void Negotiate()
        {
            RSAKeys rsaKeys = new RSAKeys();
            BaseStream.Write(rsaKeys.PublicKey);
            RSAParameters localPrivateKey = rsaKeys.PrivateKey;
            BaseStream.Flush();
            RSAParameters remotePublicKey = BaseStream.ReadRSAParameters();
            
            Random rnd = Processor.CreateRandom();
            int nbTries = 0;
            byte[] id;
            do
            {
                if(nbTries > 100)
                    throw new IOException("Can't find a different ID to use.");
                
                this.LocalID = rnd.Next();
                id = this.LocalID.GetBytes();
                
                BaseStream.WriteWrapped(RSA.Encrypt(id, remotePublicKey));
                this.RemoteID = RSA.Decrypt(BaseStream.ReadWrapped(), localPrivateKey).ToInt32();
                
                ++nbTries;
            } while(this.LocalID == this.RemoteID);
            
            byte[] key;
            byte[] iv;
            
            if(this.RemoteID < LocalID)
            {
                AES.ProduceKeyIV(out key, out iv);
                
                BaseStream.WriteWrapped(RSA.Encrypt(key, remotePublicKey));
                BaseStream.WriteWrapped(RSA.Encrypt(iv, remotePublicKey));
                BaseStream.Flush();
            }
            else
            {
                key = RSA.Decrypt(BaseStream.ReadWrapped(), localPrivateKey);
                iv = RSA.Decrypt(BaseStream.ReadWrapped(), localPrivateKey);
            }
            
            this.EncryptedStream = new OverAESStream(BaseStream, key, iv, NbIterations);
            
            this.WriteWrapped(id);
            this.Flush();
            
            if(this.ReadWrapped().ToInt32() != RemoteID)
                throw new IOException("Can't connect to remote point.");
        }
        
        protected int LocalID
        {
            get;
            private set;
        }
        
        protected int RemoteID
        {
            get;
            private set;
        }
        
        public EncryptedStream EncryptedStream
        {
            get;
            private set;
        }
        
        public override void Flush()
        {
            EncryptedStream.Flush();
        }
        
        public override int Read(byte[] data, int start, int length)
        {
            return EncryptedStream.Read(data, start, length, RemoteID.GetBytes());
        }
        
        public override void Write(byte[] data, int start, int length)
        {
            EncryptedStream.Write(LocalID.GetBytes(), 0, 4);
            EncryptedStream.Write(data, start, length);
        }
    }
}