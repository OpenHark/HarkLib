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
            
            byte[] key;
            byte[] iv;
            
            AES.ProduceKeyIV(out key, out iv);
            
            byte[] encrypted = AES.Encrypt(text.GetBytes(), key, iv);
            
            if(encrypted.ToString() == text)
                return false;
                
            byte[] decrypted = AES.Decrypt(encrypted, key, iv);
            
            if(decrypted.GetString() != text)
                return false;
            
            return true;
        }
    }
}