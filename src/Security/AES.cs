using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    /// <summary>
    /// Provides some tools for AES encryption and decryption.
    /// </summary>
    public static class AES
    {
        /// <summary>
        /// Produce a key and an initialization vector for AES.
        /// </summary>
        public static void ProduceKeyIV(out byte[] key, out byte[] iv)
        {
            using(Rijndael aes = Rijndael.Create())
            {
                key = aes.Key;
                iv = aes.IV;
            }
        }
        
        /// <summary>
        /// Create an encryption stream.
        /// </summary>
        public static CryptoStream GetEncrypter(byte[] key, byte[] iv, Stream destinationStream)
        {
            using(Rijndael aes = Rijndael.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                return new CryptoStream(destinationStream, encryptor, CryptoStreamMode.Write);
            }
        }
        
        /// <summary>
        /// Create a decryption stream.
        /// </summary>
        public static CryptoStream GetDecrypter(byte[] key, byte[] iv, Stream sourceStream)
        {
            using(Rijndael aes = Rijndael.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                return new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read);
            }
        }
        
        /// <summary>
        /// Encrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Encrypt(data, 0, data.Length, key, iv);
        }
        
        /// <summary>
        /// Encrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, int start, int length, byte[] key, byte[] iv)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(Stream cs = GetEncrypter(key, iv, ms))
                {
                    cs.Write(data, start, length);
                    cs.Flush();
                }
                return ms.ToArray();
            }
        }
        
        /// <summary>
        /// Decrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Decrypt(data, 0, data.Length, key, iv);
        }
        
        /// <summary>
        /// Decrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, int start, int length, byte[] key, byte[] iv)
        {
            using(MemoryStream r = new MemoryStream())
            using(MemoryStream ms = new MemoryStream(data, start, length))
            using(CryptoStream cs = GetDecrypter(key, iv, ms))
            {
                cs.CopyTo(r);
                
                return r.ToArray();
            }
        }
    }
}