using System.Security;
using System.IO;
using System;

using HarkLib.Security;

namespace UnitTesting.Security
{
    public class SecurityRSA : ITest
    {
        public override string Name
        {
            get
            {
                return "Security.RSA";
            }
        }
        
        public override bool Execute()
        {
            string text = "Hello!";
            
            RSAKeys keys = new RSAKeys();
            
            byte[] encryptedData = RSA.Encrypt(text.GetBytes(), keys);
            
            if(encryptedData.GetString() == text)
                return false;
                
            byte[] decryptedData = RSA.Decrypt(encryptedData, keys);
            
            if(decryptedData.GetString() != text)
                return false;
            
            return true;
        }
    }
}