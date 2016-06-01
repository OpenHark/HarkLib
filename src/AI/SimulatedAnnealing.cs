using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class SimulatedAnnealing<T>
    {
        public SimulatedAnnealing()
        {
            this.BestFoundEnergy = Double.MaxValue;
            this.BestFound = default(T);
        }
        
        public double Temperature
        {
            get;
            set;
        }
        
        public Func<T, List<T>> NeighborhoodProvider;
        public Func<T, double> EnergyProvider;
        
        public T CurrentElement
        {
            get;
            set;
        }
        
        public T BestFound
        {
            get;
            set;
        }
        public double BestFoundEnergy
        {
            get;
            set;
        }
        
        protected readonly Random Random = new Random();
        
        public void Run(int nbIterations)
        {
            double currentEnergy = EnergyProvider(CurrentElement);
            if(currentEnergy < BestFoundEnergy)
            {
                BestFoundEnergy = currentEnergy;
                BestFound = CurrentElement;
            }
            
            for(int k = 0; k < nbIterations; ++k)
            {
                Temperature = GetTemperature(k / (double)nbIterations);
                
                List<T> neighbors = NeighborhoodProvider(CurrentElement);
                T newElement = neighbors[Random.Next(0, neighbors.Count)];
                double newEnergy = EnergyProvider(newElement);
                
                if(newEnergy < BestFoundEnergy)
                {
                    BestFoundEnergy = newEnergy;
                    BestFound = newElement;
                }
                
                if(Acceptance(currentEnergy, newEnergy) >= Random.NextDouble())
                {
                    CurrentElement = newElement;
                    currentEnergy = newEnergy;
                }
            }
        }
        
        protected virtual double Acceptance(double currentEnergy, double newEnergy)
        {
            if(newEnergy < currentEnergy)
                return 1;
            
            return Math.Exp((currentEnergy - newEnergy) / Temperature);
        }
        
        protected virtual double GetTemperature(double coef)
        {
            return 1 - coef;
        }
    }
}