using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.AI
{
    public abstract class GeneticAlgorithm<T>
    {
        public GeneticAlgorithm(
            int populationSize,
            double mutationRate,
            double crossoverRate,
            int chromoLength)
        {
            PopulationSize = populationSize;
            WorstFitness = Double.MaxValue;
            CrossoverRate = crossoverRate;
            BestFitnessGenomeEver = null;
            MutationRate = mutationRate;
            ChromoLength = chromoLength;
            BestFitnessGenome = null;
            BestFitnessEver = 0;
            NbEliteCopies = 1;
            IsElitist = false;
            TotalFitness = 0;
            BestFitness = 0;
            NbEliteMax = 4;
            
            Genomes = new Genome<T>[PopulationSize];
            for(int i = 0; i < PopulationSize; ++i)
            {
                Genomes[i] = new Genome<T>(ChromoLength);
                Initialize(Genomes[i]);
            }
        }
        
        protected static readonly ReverseGenomeComparer<T> Comparer = new ReverseGenomeComparer<T>();
        protected readonly Random Random = Processor.CreateRandom();
        
        public bool IsElitist
        {
            get;
            set;
        }
        
        public int NbEliteCopies
        {
            get;
            set;
        }
        
        public int NbEliteMax
        {
            get;
            set;
        }
        
        public Genome<T>[] Genomes
        {
            get;
            protected set;
        }
        
        public double BestFitness
        {
            get;
            set;
        }
        
        public int PopulationSize
        {
            get;
            set;
        }
        
        public int ChromoLength
        {
            get;
            set;
        }
        
        public double MutationRate
        {
            get;
            set;
        }
        
        public double CrossoverRate
        {
            get;
            set;
        }

        public double TotalFitness
        {
            get;
            protected set;
        }

        public double WorstFitness
        {
            get;
            protected set;
        }

        public Genome<T> BestFitnessGenome
        {
            get;
            protected set;
        }
        
        public double BestFitnessEver
        {
            get;
            protected set;
        }
        
        public Genome<T> BestFitnessGenomeEver
        {
            get;
            protected set;
        }
        
        public double AverageFitness
        {
            get
            {
                return TotalFitness / PopulationSize;
            }
        }
        
        protected abstract void Mutate(T[] chromo, double mutationRate);
        protected abstract void Initialize(Genome<T> genome);
        
        protected virtual void Crossover(T[] parent1,  T[] parent2, out T[] child1, out T[] child2)
        {
            if(Random.NextDouble() > CrossoverRate || parent1 == parent2)
            {
                child1 = parent1;
                child2 = parent2;

                return;
            }

            // Define a crossover point
            int cp = Random.Next(0, ChromoLength - 1);
            
            child1 = new T[parent1.Length];
            child2 = new T[parent1.Length];
            
            for(int i = 0; i < cp; ++i)
            {
                child1[i] = parent1[i];
                child2[i] = parent2[i];
            }
            
            for(int i = cp; i < parent1.Length; ++i)
            {
                child1[i] = parent2[i];
                child2[i] = parent1[i];
            }
        }

        protected virtual Genome<T> TriggerRoulette()
        {
            double slice = Random.NextDouble() * TotalFitness;
            
            Genome<T> chosenGenome = Genomes[0];
            
            double fitnessAcc = 0;
            foreach(Genome<T> genome in Genomes)
            {
                fitnessAcc += genome.Fitness;
                
                if(fitnessAcc >= slice)
                {
                    chosenGenome = genome;
                    break;
                }
            }

            return chosenGenome;
        }

        protected virtual List<Genome<T>> GetBests(int nbBest, int nbCopies)
        {
            List<Genome<T>> bests = new List<Genome<T>>();
            
            for(int i = 1; i <= nbBest; ++i)
            {
                var l = Enumerable.Repeat(PopulationSize - i, nbCopies)
                    .Select(id => Genomes[id])
                    .ToList();
                    
                bests.AddRange(l);
            }
            
            return bests;
        }

        protected virtual void CalculateRemarkableValues()
        {
            // Because Genomes are sorted :
            //   First = worst
            //   Last = best
            
            WorstFitness = Math.Min(Genomes.First().Fitness, WorstFitness);
            TotalFitness = Genomes.Sum(g => g.Fitness);
            
            double bestGenomeFitness = Genomes.Last().Fitness;
            if(bestGenomeFitness > BestFitness)
            {
                BestFitnessGenome = Genomes.Last();
                BestFitness = bestGenomeFitness;
                
                if(bestGenomeFitness > BestFitnessEver)
                {
                    BestFitnessGenomeEver = BestFitnessGenome;
                    BestFitnessEver = bestGenomeFitness;
                }
            }
        }

        protected virtual void ResetRemarkableValues()
        {
            TotalFitness = -1;
            WorstFitness = -1;
            BestFitness = -1;
        }
        
        public virtual Genome<T>[] Epoch(Genome<T>[] oldGenomes = null)
        {
            Genomes = oldGenomes ?? Genomes;
            
            // Sort for the roulette
            Array.Sort(Genomes, Comparer);
            
            ResetRemarkableValues();
            CalculateRemarkableValues();
            
            // Copy the genomes
            Genomes = Genomes.Select(g => new Genome<T>(g)).ToArray();
            
            List<Genome<T>> population;
            if(IsElitist)
                population = GetBests(Math.Min(NbEliteMax, PopulationSize), NbEliteCopies);
            else
                population = new List<Genome<T>>();
            
            while(population.Count < PopulationSize)
            {
                Genome<T> parent1 = TriggerRoulette();
                Genome<T> parent2 = TriggerRoulette();
                
                T[] child1, child2;
                Crossover(parent1.Values, parent2.Values, out child1, out child2);
                
                Mutate(child1, MutationRate);
                Mutate(child2, MutationRate);
                
                population.Add(new Genome<T>(child1, 0));
                population.Add(new Genome<T>(child2, 0));
            }
            
            Genomes = population.ToArray();
            
            return Genomes;
        }
    }
}