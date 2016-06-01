using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class GeneticAlgorithmDouble : GeneticAlgorithm<double>
    {
        public GeneticAlgorithmDouble(
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
        
        protected override void Mutate(double[] chromo, double mutationRate)
        {
            for(int i = 0; i < chromo.Length; ++i)
                if(Random.NextDouble() < mutationRate)
                    chromo[i] += (Random.NextDouble() - Random.NextDouble()) * MaxPerturbation;
        }
        
        protected override void Initialize(Genome<double> genome)
        {
            for(int i = 0; i < ChromoLength; ++i)
                genome.Values[i] = Random.NextDouble() - Random.NextDouble();
        }
    }
}