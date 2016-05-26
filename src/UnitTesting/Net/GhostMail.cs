using System;

using HarkLib.Net;

namespace UnitTesting.Net
{
    public class NetGhostMail : ITest
    {
        public override string Name
        {
            get
            {
                return "Net.GhostMail";
            }
        }
        
        public override bool Execute()
        {
            if(IsVerbose)
            {
                GhostMail gm = new GhostMail()
                {
                    DestinationEMail = "source.user@gmail.com",
                    DestinationUser = "Source User",
                    SourceUser = "Dest User",
                    SourceEMail = "dest.user@destination.domain.com",
                    Subject = "subject",
                    Content = "content..."
                };
                
                gm.Send();
                
                Console.WriteLine("EMail sent.");
            }
            
            return true;
        }
    }
}