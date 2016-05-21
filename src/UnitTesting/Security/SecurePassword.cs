using System.Diagnostics;
using System.Security;
using System.Linq;
using System;

using HarkLib.Security;

namespace UnitTesting.Security
{
    public class SecuritySecurePassword : ITest
    {
        public override string Name
        {
            get
            {
                return "Security.SecurePassword";
            }
        }
        
        private TimeSpan Execute(int nbIterations, out SecurePassword sp)
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            sp = new SecurePassword(
                password : ProvideSecureString(),
                nbHash : nbIterations,
                autoClear : true
            );
            sw.Stop();
            
            return sw.Elapsed;
        }
        
        private SecureString ProvideSecureString()
        {
            SecureString ss = new SecureString();
            "MyPassword".ForEach(ss.AppendChar);
            ss.MakeReadOnly();
            
            return ss;
        }
        
        public override bool Execute()
        {
            SecurePassword sp;
            TimeSpan it10000 = Execute(10000, out sp);
            
            byte[] result = new byte[]
            { // "MyPassword" hashed 10000 times with SHA-256
                205, 145, 243, 77, 50, 200, 192, 188, 86, 174,
                137, 126, 223, 26, 124, 17, 175, 42, 231, 102,
                86, 106, 249, 206, 3, 115, 220, 114, 251, 203,
                107, 112, 192, 168, 46, 122, 163, 219, 30, 211,
                83, 254, 146, 140, 205, 36, 35, 235, 229, 249,
                170, 193, 117, 76, 250, 133, 144, 96, 225, 241,
                250, 43, 33, 170
            };
            
            if(result.Length != sp.Bytes.Length)
                return false;
            
            if(!sp.Bytes.All((b,i) => result[i] == b))
                return false;
            
            // Without auto-clean
            sp = new SecurePassword(
                password : ProvideSecureString(),
                nbHash : 10000,
                autoClear : false
            );
            
            if(result.Length != sp.Bytes.Length)
                return false;
            
            if(!sp.Bytes.All((b,i) => result[i] == b))
                return false;
            
            if(IsVerbose)
            {
                WriteLine("   10 000 iterations : " + it10000);
                
                TimeSpan it100000 = Execute(100000, out sp);
                WriteLine("  100 000 iterations : " + it100000);
                
                TimeSpan it1000000 = Execute(1000000, out sp);
                WriteLine("1 000 000 iterations : " + it1000000);
            }
            
            return true;
        }
    }
}