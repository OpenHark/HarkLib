using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System;

using HarkLib.AI;

namespace UnitTesting.AI
{
    public class AISimulatedAnnealing : ITest
    {
        public override string Name
        {
            get
            {
                return "AI.SimulatedAnnealing";
            }
        }
        
        protected List<string> Neighbors(string value)
        {
            List<string> neighbors = new List<string>();
            
            for(int i = 0; i < value.Length; ++i)
            {
                if(value[i] > 0)
                {
                    neighbors.Add(new StringBuilder(value).Remove(i, 1).Insert(i, (char)(value[i] - 1)).ToString());
                }
                else
                    neighbors.Add(new StringBuilder(value).Remove(i, 1).Insert(i, (char)255).ToString());
                if(value[i] < 255)
                {
                    neighbors.Add(new StringBuilder(value).Remove(i, 1).Insert(i, (char)(value[i] + 1)).ToString());
                }
                else
                    neighbors.Add(new StringBuilder(value).Remove(i, 1).Insert(i, (char)0).ToString());
            }
            
            return neighbors;
        }
        
        public override bool Execute()
        {
            string aim = "Hello friends";
            int dim = aim.Length;
            
            Func<string, double> fn1 = (s => s.Select((b,i) => ((int)b - aim[i]) * 2 * ((int)b - aim[i])).Sum());
            Func<string, double> fn = (s => fn1(s));
            
            SimulatedAnnealing<string> sa = new SimulatedAnnealing<string>()
            {
                NeighborhoodProvider = Neighbors,
                EnergyProvider = fn,
                CurrentElement = "^ùsprôqù;rqô;ùvere!wlpc\"!".Substring(0, dim)
            };
            
            for(int i = 0; i < 100; ++i)
            {
                sa.Run(100);
                sa.CurrentElement = sa.BestFound;
            }
            
            if(IsVerbose)
                Console.WriteLine(" ::!:: " + sa.BestFound);
            
            if(sa.BestFound != aim)
                return false;
            
            return true;
        }
    }
}