using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

namespace HarkLib.Security
{
    public static class AES
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using(MemoryStream ms = new MemoryStream())
                {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data);
                        cs.Flush();
                    }
                    return ms.ToArray();
                }
            }
        }
        
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

                using(MemoryStream r = new MemoryStream())
                using(MemoryStream ms = new MemoryStream(data))
                using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    cs.CopyTo(r);
                    
                    return r.ToArray();
                }
            }
        }
    }
}