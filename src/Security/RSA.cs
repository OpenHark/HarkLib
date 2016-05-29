using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    public static class RSA
    {
        public static byte[] Encrypt(byte[] data, RSAParameters publicKey, bool padding = true)
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                
                return rsa.Encrypt(data, padding);
            }
        }
        public static byte[] Encrypt(byte[] data, RSAKeys keys, bool padding = true)
        {
            return Encrypt(data, keys.PublicKey, padding);
        }
        
        public static byte[] Decrypt(byte[] data, RSAParameters privateKey, bool padding = true)
        {
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                
                return rsa.Decrypt(data, padding);
            }
        }
        public static byte[] Decrypt(byte[] data, RSAKeys keys, bool padding = true)
        {
            return Decrypt(data, keys.PrivateKey, padding);
        }
    }
}