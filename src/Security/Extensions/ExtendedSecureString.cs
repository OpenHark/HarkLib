using System.Runtime.InteropServices;
using System.Security;
using System;

namespace System.Security
{
    public static class ExtendedSecureString
    {
        public static byte[] GetBytes(this SecureString secureString)
        {
            IntPtr unmanaged = IntPtr.Zero;
            try
            {
                unmanaged = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanaged).GetBytes();
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
            }
        }
    }
}