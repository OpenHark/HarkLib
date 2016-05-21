using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

using HarkLib.Security;

namespace UnitTesting.Security
{
    public class SecurityAES : ITest
    {
        public override string Name
        {
            get
            {
                return "Security.AES";
            }
        }
        
        public override bool Execute()
        {
            string text = "Hello!";
            
            using(Aes aes = Aes.Create())
            {
                byte[] encrypted = AES.Encrypt(text.GetBytes(), aes.Key, aes.IV);
                
                if(encrypted.ToString() == text)
                    return false;
                    
                byte[] decrypted = AES.Decrypt(encrypted, aes.Key, aes.IV);
                
                if(decrypted.GetString() != text)
                    return false;
            }
            
            return true;
        }
    }
}