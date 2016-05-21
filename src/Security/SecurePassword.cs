using System.Security.Cryptography;
using System.Security;
using System;

namespace HarkLib.Security
{
    /// <summary>
    /// Secure form of a password for use.
    /// </summary>
    public class SecurePassword : IDisposable
    {
        /// <summary>
        /// Create a secure form of the password for use.
        /// </summary>
        /// <note>
        /// The <i>password</i> is disposed in the process.
        /// </note>
        /// <param name="password">Password to secure for use.</param>
        /// <param name="nbHash">Number of hash to compute on the password.</param>
        /// <param name="hasher">Hash algorithm to use.</param>
        /// <param name="autoClear">Defines if the intermediary values must be cleared in the process.</param>
        public SecurePassword(
            SecureString password,
            int nbHash = 200000,
            bool autoClear = true,
            HashAlgorithm hasher = null)
        {
            hasher = hasher ?? new SHA512Managed();
            
            byte[] pwd = password.GetBytes();
            password.Dispose();
            byte[] data = hasher.ComputeHash(pwd);
            pwd.Clear();
            
            if(autoClear)
            {
                byte[] tempData = null;
                for(int i = 0; i < nbHash - 1; ++i)
                {
                    if((i & 1) == 0)
                    {
                        tempData = hasher.ComputeHash(data);
                        data.Clear();
                    }
                    else
                    {
                        data = hasher.ComputeHash(tempData);
                        tempData.Clear();
                    }
                }
                
                this.bytes = ((nbHash - 1) & 1) == 0 ? data : tempData;
            }
            else
            {
                for(int i = 0; i < nbHash - 1; ++i)
                    data = hasher.ComputeHash(data);
            
                this.bytes = data;
            }
        }
        
        private readonly byte[] bytes;
        
        /// <summary>
        /// Secure form of the password.
        /// It is the result of the <i>nbHash</i> hash computed
        /// on the password.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return bytes;
            }
        }
        
        public void Dispose()
        {
            this.Bytes.Clear();
        }
    }
}