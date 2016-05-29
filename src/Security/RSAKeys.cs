using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    public class RSAKeys
    {
        public RSAKeys()
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                this.PrivateKey = rsa.ExportParameters(true);
                this.PublicKey = rsa.ExportParameters(false);
            }
        }
        
        public RSAParameters PublicKey
        {
            get;
            private set;
        }
        
        public RSAParameters PrivateKey
        {
            get;
            private set;
        }
    }
}