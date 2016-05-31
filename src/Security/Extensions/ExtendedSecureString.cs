using System.Runtime.InteropServices;
using System.Security;
using System;

namespace System.Security
{
    public static class ExtendedSecureString
    {
        /// <summary>
        /// Extract the byte array beneath the SecureString class.
        /// </summary>
        public static byte[] GetBytes(this SecureString secureString)
        {
            return secureString.GetString().GetBytes();
        }
        
        /// <summary>
        /// Extract the string value beneath the SecureString class.
        /// </summary>
        public static string GetString(this SecureString secureString)
        {
            IntPtr unmanaged = IntPtr.Zero;
            try
            {
                unmanaged = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanaged);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
            }
        }
    }
}