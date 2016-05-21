using System.Diagnostics;
using System.Linq;
using System.Net;
using System;

namespace UnitTesting
{
    public abstract class ITest
    {
        public abstract bool Execute();
        
        public abstract string Name
        {
            get;
        }
        
        public bool IsVerbose
        {
            get;
            set;
        }
        
        protected void WriteLine(object obj)
        {
            if(IsVerbose)
                Console.WriteLine(obj);
        }
    }
}