using System.Diagnostics;
using System.Linq;
using System.Net;
using System;

using HarkLib.AI;

namespace UnitTesting.AI
{
    public class AIGeneticAlgorithm : ITest
    {
        public override string Name
        {
            get
            {
                return "AI.GeneticAlgorithm";
            }
        }
        
        public class GeneticAlgorithmByte : GeneticAlgorithm<byte>
        {
            public GeneticAlgorithmByte(
                int populationSize,
                double mutationRate,
                double crossoverRate,
                int chromoLength)
                : base(populationSize, mutationRate, crossoverRate, chromoLength)
            {
                this.MaxPerturbation = 0.3;
            }
            
            public double MaxPerturbation
            {
                get;
                set;
            }
            
            protected override void Mutate(byte[] chromo, double mutationRate)
            {
                for(int i = 0; i < chromo.Length; ++i)
                    if(Random.NextDouble() < mutationRate)
                        chromo[i] += (byte)((Random.NextDouble() - Random.NextDouble()) * MaxPerturbation);
            }
            
            protected override void Initialize(Genome<byte> genome)
            {
                for(int i = 0; i < ChromoLength; ++i)
                    genome.Weights[i] = (byte)Random.Next(0, 255);
            }
        }
        
        public override bool Execute()
        {
            string aim = "Hello friends";
            int nbChars = aim.Length;
            
            GeneticAlgorithm<byte> ga = new GeneticAlgorithmByte(50, 1.0 / (nbChars - 1), 0.7, nbChars)
            {
                MaxPerturbation = 5,
                IsElitist = true
            };
            
            for(int k = 0; k < 1000; ++k)
            {
                foreach(Genome<byte> g in ga.Epoch())
                {
                    g.Fitness = g
                        .Weights
                        .Select((b,i) => 254 * 254 - ((int)b - aim[i]) * ((int)b - aim[i]))
                        .Sum() / (double)(254 * 254 * nbChars);
                }
            }
            
            if(IsVerbose)
            {
                Console.WriteLine("   BEST : " + ga.BestFitnessEver);
                Console.WriteLine(ga.BestFitnessGenomeEver.Weights.Select(b => (char)b).Aggregate("", (c1,c2) => c1 + c2));
            }
            
            if(ga.BestFitnessEver < 1)
                return false;
            
            return true;
        }
    }
}