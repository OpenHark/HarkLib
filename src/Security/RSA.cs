using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    /// <summary>
    /// Provides some tools for RSA encryption and decryption.
    /// </summary>
    public static class RSA
    {
        /// <summary>
        /// Encrypt data with the public key provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, RSAParameters publicKey, bool padding = true)
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                
                return rsa.Encrypt(data, padding);
            }
        }
        
        /// <summary>
        /// Encrypt data with the public part of the key provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, RSAKeys keys, bool padding = true)
        {
            return Encrypt(data, keys.PublicKey, padding);
        }
        
        /// <summary>
        /// Decrypt data with the private key provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, RSAParameters privateKey, bool padding = true)
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                
                return rsa.Decrypt(data, padding);
            }
        }
        
        /// <summary>
        /// Decrypt data with the private part of the key provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, RSAKeys keys, bool padding = true)
        {
            return Decrypt(data, keys.PrivateKey, padding);
        }
    }
}