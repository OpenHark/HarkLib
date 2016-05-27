using System;

using HarkLib.Net;

namespace UnitTesting.Net
{
    public class NetWebResource : ITest
    {
        public override string Name
        {
            get
            {
                return "Net.WebResource";
            }
        }
        
        public override bool Execute()
        {
            if(IsVerbose)
            {
                // The content of the web page must me something similar to :
                // http://liris.cnrs.fr/~amille/enseignements/MasterCode/IC_IA/
                WebDirectory wd = new WebDirectory("...url...");
                
                Console.WriteLine("Directories : ");
                foreach(var w in wd.Directories)
                    Console.WriteLine(w.Name + " :: " + w.Url);
                    
                Console.WriteLine("Files : ");
                foreach(var w in wd.Files)
                    Console.WriteLine(w.Name + " :: " + w.Url);
                    
                Console.WriteLine("Directories & Files : ");
                foreach(var w in wd.Resources)
                    Console.WriteLine(w.Name + " :: " + w.Url);
            }
            
            return true;
        }
    }
}