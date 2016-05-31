using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    /// <summary>
    /// This is a wrapping class for RSA keys.
    /// It contains public and private keys.
    /// </summary>
    public class RSAKeys
    {
        /// <summary>
        /// Create a pair of RSA public and private keys.
        /// </summary>
        public RSAKeys()
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                this.PrivateKey = rsa.ExportParameters(true);
                this.PublicKey = rsa.ExportParameters(false);
            }
        }
        
        /// <summary>
        /// Public key of the RSA system.
        /// It is used for RSA encryption.
        /// </summary>
        /// <note>
        /// You can share this key wherever you want.
        /// </note>
        public RSAParameters PublicKey
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Private key of the RSA system.
        /// It is used for RSA decryption.
        /// </summary>
        /// <note>
        /// Keep this key personal. Do not share it or you
        /// waste the aim of this asymetric system.
        /// </note>
        public RSAParameters PrivateKey
        {
            get;
            private set;
        }
    }
}