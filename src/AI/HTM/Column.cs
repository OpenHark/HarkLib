using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI.old
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Column : IInput
    {
        public Column()
        {
            this.Synapses = new List<Synapse>();
            this.Boost = 1.0;
        }
        
        private bool _Value = false;
        public bool Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = true;
                
                DutyCycle = (DutyCycle * nbDutyCycle + (_Value ? 1 : 0)) / (nbDutyCycle + 1);
                ++nbDutyCycle;
            }
        }
        
        protected int nbDutyCycle = 0;
        public double DutyCycle
        {
            get;
            protected set;
        }
        
        protected int nbOverlapDutyCycle = 0;
        public double OverlapDutyCycle
        {
            get;
            protected set;
        }
        
        public List<Cell> Cells
        {
            get;
            set;
        }
        
        public Location Location
        {
            get;
            set;
        }
        
        private double _Overlap;
        public double Overlap
        {
            get
            {
                return _Overlap;
            }
            set
            {
                _Overlap = value;
                
                OverlapDutyCycle = (OverlapDutyCycle * nbOverlapDutyCycle + _Overlap) / (nbOverlapDutyCycle + 1);
                ++nbOverlapDutyCycle;
            }
        }
        
        public List<Synapse> Synapses
        {
            get;
            set;
        }
        
        // >= 1
        public double Boost
        {
            get;
            set;
        }
    }
}