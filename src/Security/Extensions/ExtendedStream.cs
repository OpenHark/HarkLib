using System.Security.Cryptography;
using System.Collections.Generic;
using System.Security;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class ExtendedStream
    {
        private static void NoNull(ref RSAParameters parameters)
        {
            parameters.D = parameters.D ?? new byte[0];
            parameters.DP = parameters.DP ?? new byte[0];
            parameters.DQ = parameters.DQ ?? new byte[0];
            parameters.Exponent = parameters.Exponent ?? new byte[0];
            parameters.InverseQ = parameters.InverseQ ?? new byte[0];
            parameters.Modulus = parameters.Modulus ?? new byte[0];
            parameters.P = parameters.P ?? new byte[0];
            parameters.Q = parameters.Q ?? new byte[0];
        }
        
        private static void WithNull(ref RSAParameters parameters)
        {
            parameters.D = parameters.D.Length == 0 ? null : parameters.D;
            parameters.DP = parameters.DP.Length == 0 ? null : parameters.DP;
            parameters.Exponent = parameters.Exponent.Length == 0 ? null : parameters.Exponent;
            parameters.InverseQ = parameters.InverseQ.Length == 0 ? null : parameters.InverseQ;
            parameters.Modulus = parameters.Modulus.Length == 0 ? null : parameters.Modulus;
            parameters.P = parameters.P.Length == 0 ? null : parameters.P;
            parameters.Q = parameters.Q.Length == 0 ? null : parameters.Q;
        }
        
        /// <summary>
        /// Write RSA parameters, i.e. D, DP, DQ, Exponnent,
        /// Inverse Q, Modulus P and Q.
        /// </summary>
        public static void Write(this Stream stream, RSAParameters parameters)
        {
            NoNull(ref parameters);
            
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
            
            WithNull(ref parameters);
            return parameters;
        }
    }   
}