using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Synapse
    {
        public bool IsConnected
        {
            get;
            set;
        }
        
        private double _Permanence;
        public double Permanence
        {
            get
            {
                return _Permanence;
            }
            set
            {
                _Permanence = Math.Min(Math.Max(value, 0.0), 1.0);
            }
        }
        
        public void Connect(double threshold)
        {
            IsConnected = Permanence > threshold;
        }
    }
}