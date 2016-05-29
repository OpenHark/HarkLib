using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    public static class AES
    {
        public static void ProduceKeyIV(out byte[] key, out byte[] iv)
        {
            using(Aes aes = Aes.Create())
            {
                key = aes.Key;
                iv = aes.IV;
            }
        }
        
        public static CryptoStream GetEncrypter(byte[] key, byte[] iv, Stream destinationStream)
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                return new CryptoStream(destinationStream, encryptor, CryptoStreamMode.Write);
            }
        }
        
        public static CryptoStream GetDecrypter(byte[] key, byte[] iv, Stream sourceStream)
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

                return new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read);
            }
        }
        
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(Stream cs = GetEncrypter(key, iv, ms))
                {
                    cs.Write(data);
                    cs.Flush();
                }
                return ms.ToArray();
            }
        }
        
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(MemoryStream r = new MemoryStream())
            using(MemoryStream ms = new MemoryStream(data))
            using(CryptoStream cs = GetDecrypter(key, iv, ms))
            {
                cs.CopyTo(r);
                
                return r.ToArray();
            }
        }
    }
}