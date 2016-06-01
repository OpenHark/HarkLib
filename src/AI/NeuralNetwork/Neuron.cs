using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class Neuron
    {
        public Neuron(int nbInputs, Random weightInitializor = null)
        {
            this.NbInputs = nbInputs;
            this.Weights = new double[nbInputs + 1];
            
            if(weightInitializor != null)
            {
                for(int i = 0; i < this.Weights.Length; ++i)
                    this.Weights[i] = weightInitializor.NextDouble() - weightInitializor.NextDouble();
            }
        }
        
        public int NbInputs
        {
            get;
            protected set;
        }

        public double[] Weights
        {
            get;
            protected set;
        }
        
        protected static double Sigmoid(double netinput, double response)
        {
            return ( 1 / ( 1 + Math.Exp(-netinput / response)));
        }
        
        public double GetOutput(double[] inputs)
        {
            double netInput = 0;
            
            for(int i = 0; i < NbInputs - 1; ++i)
                netInput += Weights[i] * inputs[i];
            
            netInput += Weights[NbInputs - 1] * (-1);
            
            return Sigmoid(netInput, 1);
        }
    }
}