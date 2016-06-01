using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class Genome<T>
    {
        public Genome(T[] values, double fitness)
        {
            this.Values = values;
            this.Fitness = fitness;
        }
        public Genome(int nbValues)
        {
            this.Values = new T[nbValues];
            this.Fitness = 0;
        }
        public Genome(Genome<T> genome)
        {
            this.Values = new T[genome.Values.Length];
            for(int i = 0; i < genome.Values.Length; ++i)
                this.Values[i] = genome.Values[i];
            this.Fitness = genome.Fitness;
        }
        
        public T[] Values
        {
            get;
            private set;
        }
        
        public double Fitness
        {
            get;
            set;
        }
    }
}