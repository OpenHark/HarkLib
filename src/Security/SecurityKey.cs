using System.Security.Cryptography;
using System.Security;
using System.IO;
using System;

using HarkLib.Core;

namespace HarkLib.Security
{
    public static class SecurityKey
    {
        public static byte[][] Slice(string password, byte[] salt, int nbIterations, params int[] size)
        {
            byte[][] slices = new byte[size.Length][];
            
            using(Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(password, salt, nbIterations))
            {
                for(int i = 0; i < slices.Length; ++i)
                    slices[i] = derive.GetBytes(size[i]);
            }
            
            return slices;
        }
    }
}