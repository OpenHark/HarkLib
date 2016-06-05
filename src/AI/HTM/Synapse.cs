using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Synapse : IInput
    {
        public bool IsConnected
        {
            get;
            set;
        }
        
        public Location Location
        {
            get
            {
                return Input.Location;
            }
        }
        
        public IInput Input
        {
            get;
            set;
        }
        
        public bool Value
        {
            get
            {
                return Input.Value;
            }
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
                Connect();
            }
        }
        
        public double ConnectionThreshold
        {
            get;
            set;
        }
        
        public void Connect(double threshold)
        {
            ConnectionThreshold = threshold;
            IsConnected = Permanence > threshold;
        }
        public void Connect()
        {
            IsConnected = Permanence > ConnectionThreshold;
        }
    }
}