using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    public static class OverAES
    {
        public static byte[] Hash(byte[] data, int nbIterations)
        {
            return Hash(data, 0, data.Length, nbIterations);
        }
        public static byte[] Hash(byte[] data, int start, int length, int nbIterations)
        {
            SHA512Managed hasher = new SHA512Managed();
            for(int i = 0; i < nbIterations; ++i)
                data = hasher.ComputeHash(data, start, length);
            return data;
        }
        
        public static void Transform(byte[] data, byte[] key, int nbIterations)
        {
            Transform(data, 0, data.Length, key, nbIterations);
        }
        public static void Transform(byte[] data, int start, int length, byte[] key, int nbIterations)
        {
            byte[] hkey = Hash(key, nbIterations);
            
            for(int i = start; i < length; ++i)
                data[i] ^= hkey[i % hkey.Length];
        }
        
        /// <summary>
        /// Encrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, int nbIterations)
        {
            return Encrypt(data, 0, data.Length, key, iv, nbIterations);
        }
        
        /// <summary>
        /// Encrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Encrypt(byte[] data, int start, int length, byte[] key, byte[] iv, int nbIterations)
        {
            byte[] encrypted = AES.Encrypt(data, start, length, key, iv);
            Transform(encrypted, key, nbIterations);
            return encrypted;
        }
        
        /// <summary>
        /// Decrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, int nbIterations)
        {
            return Decrypt(data, 0, data.Length, key, iv, nbIterations);
        }
        
        /// <summary>
        /// Decrypt data with the key and the initialization vector provided.
        /// </summary>
        public static byte[] Decrypt(byte[] data, int start, int length, byte[] key, byte[] iv, int nbIterations)
        {
            Transform(data, start, length, key, nbIterations);
            return AES.Decrypt(data, start, length, key, iv);
        }
    }
}