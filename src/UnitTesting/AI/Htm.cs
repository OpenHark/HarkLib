using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System;

using HarkLib.AI;

namespace UnitTesting.AI
{
    public class AIHtm : ITest
    {
        public override string Name
        {
            get
            {
                return "AI.Htm";
            }
        }
        
        public override bool Execute()
        {
            int nbX = 16;
            int nbY = 1;
            
            int[][,] INPUTS = new int[][,]
            {
                new int[16, 1]
                {
                    { 1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }, {  1 }
                },
                new int[16, 1]
                {
                    { 0 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }
                },
                new int[16, 1]
                {
                    { 1 }, { 0 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }
                },
                new int[16, 1]
                {
                    { 1 }, { 1 }, { 0 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }, {  0 }, {  0 }, {  0 }, {  0 }, {  1 }, {  1 }
                }
            };
            
            /*
            int[,] A = new int[16, 16]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            
            int[,] B = new int[16, 16]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            
            int[,] Empty = new int[16, 16]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };*/
            
            
            Inputs inputs = new Inputs(nbX, nbY);
            inputs.SetInput(INPUTS[0]);
            
            
            Region region = new Region(nbX, nbY)
            {
                ConnectedPermanence = 0.7,
                InitialRandomPermanenceMax = 0.1,
                InitialNbSynapses = 30,
                InitialDistanceBiasDivider = 10,
                InhibitionRadius = 5,
                MinOverlap = 1,
                DesiredLocalActivity = 1,
                PermanenceInc = 0.05,
                PermanenceDec = 0.05,
                BoostStep = 0.01
            };
            Console.WriteLine(inputs.Values.Count);
            region.Initialize(inputs.Values, 15, 0.7);
            
            int id = 0;
            while(true)
            {
                int max = id + 3;
                for(int i = id; i < max; ++i)
                {
                    int[,] inp;
                    
                    inp = INPUTS[i % INPUTS.GetLength(0)];
                    
                    inputs.SetInput(inp);
                    
                    region.Phase1Overlap();
                    region.Phase2Inhibition();
                    region.Phase3Learning();
                    
                    Console.WriteLine(" :::: " + i);
                    DisplayOutput(region);
                    Console.WriteLine("************");
                    for(int y = 0; y < inp.GetLength(1); ++y)
                    {
                        for(int x = 0; x < inp.GetLength(0); ++x)
                            Console.Write((inp[x,y]) + " ");
                        Console.WriteLine();
                    }
                }
                id += 3;
                Console.ReadLine();
            }
            
            //return true;
        }
        
        protected void DisplayOutput(Region region)
        {
            //bool[,] output = region.GetOutputMap();
            bool[,] output = region.GetOutputMap();
            Console.WriteLine("************");
            for(int y = 0; y < output.GetLength(1); ++y)
            {
                for(int x = 0; x < output.GetLength(0); ++x)
                    Console.Write((output[x,y] ? 1 : 0) + " ");
                Console.WriteLine();
            }
            output = region.GetResultingInputMap();
            Console.WriteLine("************");
            for(int y = 0; y < output.GetLength(1); ++y)
            {
                for(int x = 0; x < output.GetLength(0); ++x)
                    Console.Write((output[x,y] ? 1 : 0) + " ");
                Console.WriteLine();
            }
        }
    }
}