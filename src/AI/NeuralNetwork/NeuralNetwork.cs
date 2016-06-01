using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class NeuralNetwork
    {
        public NeuralNetwork(
            int nbInputs,
            int nbOutputs,
            int nbHiddenLayers,
            int nbNeuronsPerLayer)
        {
            this.NbOutputs = nbOutputs;
            this.NbInputs = nbInputs;
            
            this.Layers = new NeuralLayer[nbHiddenLayers + 1];
            
            // First layer
            this.Layers[0] = new NeuralLayer(nbNeuronsPerLayer, nbInputs);
            
            // Middle layers
            for(int i = 1; i < nbHiddenLayers; ++i)
                this.Layers[i] = new NeuralLayer(nbNeuronsPerLayer, nbNeuronsPerLayer);
                
            // Last layer - providing the output
            this.Layers[nbHiddenLayers] = new NeuralLayer(nbOutputs, nbNeuronsPerLayer);
            
            this.NbWeights = nbOutputs + nbNeuronsPerLayer * ((nbHiddenLayers - 1) * (nbNeuronsPerLayer + 1) + nbInputs + nbOutputs + 1);
        }
        
        public NeuralLayer[] Layers
        {
            get;
            private set;
        }
        
        public int NbInputs
        {
            get;
            private set;
        }
        
        public int NbOutputs
        {
            get;
            private set;
        }
        
        public int NbWeights
        {
            get;
            private set;
        }
        
        public double[] Weights
        {
            get
            {
                double[] result = new double[NbWeights];
                int index = 0;
                for(int i = 0; i < Layers.Length; ++i)
                    for(int j = 0; j < Layers[i].Neurons.Length; ++j)
                        for(int k = 0; k < Layers[i].Neurons[j].Weights.Length; ++k)
                            result[index++] = Layers[i].Neurons[j].Weights[k];
                return result;
            }
            set
            {
                int index = 0;
                for(int i = 0; i < Layers.Length; ++i)
                    for(int j = 0; j < Layers[i].Neurons.Length; ++j)
                        for(int k = 0; k < Layers[i].Neurons[j].Weights.Length; ++k)
                        {
                            Layers[i].Neurons[j].Weights[k] = value[index++];
                        }
            }
        }
        
        public double[] Update(double[] inputs)
        {
            double[] outputs = null;
            
            foreach(NeuralLayer layer in Layers)
            {
                outputs = new double[layer.Neurons.Length];
                
                for(int i = 0; i < outputs.Length; ++i)
                    outputs[i] = layer.Neurons[i].GetOutput(inputs);
                
                inputs = outputs;
            }
            
            return outputs;
        }
    }
}