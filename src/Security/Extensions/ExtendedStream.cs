using System.Security.Cryptography;
using System.Collections.Generic;
using System.Security;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class ExtendedStream
    {
        /// <summary>
        /// Write RSA parameters, i.e. D, DP, DQ, Exponnent,
        /// Inverse Q, Modulus P and Q.
        /// </summary>
        public static void Write(this Stream stream, RSAParameters parameters)
        {
            stream.WriteWrapped(parameters.D);
            stream.WriteWrapped(parameters.DP);
            stream.WriteWrapped(parameters.DQ);
            stream.WriteWrapped(parameters.Exponent);
            stream.WriteWrapped(parameters.InverseQ);
            stream.WriteWrapped(parameters.Modulus);
            stream.WriteWrapped(parameters.P);
            stream.WriteWrapped(parameters.Q);
        }
        
        /// <summary>
        /// Read RSA parameters, i.e. D, DP, DQ, Exponnent,
        /// Inverse Q, Modulus P and Q.
        /// </summary>
        public static RSAParameters ReadRSAParameters(this Stream stream)
        {
            RSAParameters parameters = new RSAParameters();
            
            parameters.D = stream.ReadWrapped();
            parameters.DP = stream.ReadWrapped();
            parameters.DQ = stream.ReadWrapped();
            parameters.Exponent = stream.ReadWrapped();
            parameters.InverseQ = stream.ReadWrapped();
            parameters.Modulus = stream.ReadWrapped();
            parameters.P = stream.ReadWrapped();
            parameters.Q = stream.ReadWrapped();
            
            return parameters;
        }
    }   
}