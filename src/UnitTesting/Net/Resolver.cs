using System;

using HarkLib.Net;

namespace UnitTesting.Net
{
    public class NetResolver : ITest
    {
        public override string Name
        {
            get
            {
                return "Net.Resolver";
            }
        }
        
        public override bool Execute()
        {
            if(IsVerbose)
            {
                string domain = "gmail.com";
                
                Console.WriteLine("Records for " + domain);
                foreach(string s in Resolver.GetRecords(domain))
                    Console.WriteLine(s);
                    
                Console.WriteLine("Mx Records for " + domain);
                foreach(string s in Resolver.GetMxRecords(domain))
                    Console.WriteLine(s);
            }
            
            return true;
        }
    }
}