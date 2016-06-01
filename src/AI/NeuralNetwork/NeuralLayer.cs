using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class NeuralLayer
    {
        public NeuralLayer(int nbNeurons, int nbInputsPerNeuron)
        {
            this.Neurons = new Neuron[nbNeurons];
            
            for(int i = 0; i < nbNeurons; ++i)
                this.Neurons[i] = new Neuron(nbInputsPerNeuron);
        }
        public NeuralLayer(IEnumerable<Neuron> Neurons)
        {
            this.Neurons = Neurons.ToArray();
        }
        
        public Neuron[] Neurons
        {
            get;
            protected set;
        }
    }
}