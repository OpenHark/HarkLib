using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class Inputs
    {
        public Inputs(int sizeX, int sizeY)
        {
            Values = new List<IInput>();
            InputsValue = new Input[sizeX, sizeY];
            
            for(int x = 0; x < sizeX; ++x)
            for(int y = 0; y < sizeY; ++y)
            {
                Input input = new Input()
                {
                    Value = false,
                    Location = new Location()
                    {
                        X = x,
                        Y = y
                    }
                };
                
                this.Values.Add(input);
                this.InputsValue[x, y] = input;
            }
        }
        
        public void SetInput(int[,] values)
        {
            for(int x = 0; x < values.GetLength(0); ++x)
            for(int y = 0; y < values.GetLength(1); ++y)
                InputsValue[x, y].Value = values[x, y] != 0;
        }
        
        public Input[,] InputsValue
        {
            get;
            set;
        }
        
        public List<IInput> Values
        {
            get;
            set;
        }
        
        public class Input : IInput
        {
            public bool Value
            {
                get;
                set;
            }
            
            public Location Location
            {
                get;
                set;
            }
        }
    }
}