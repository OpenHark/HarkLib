using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Cell
    {
        
        
        public CellOutputState OutputState
        {
            get;
            set;
        }
        
        // Reception : Feed-forward inputs of the column
        // They are common to all cells in a column
        public DendriteSegment ProximalDendriteSegments
        {
            get;
            set;
        }
        
        // Reception : Lateral inputs
        public DendriteSegment DistalDendriteSegments
        {
            get;
            set;
        }
    }
    
    public enum CellOutputState
    {
        LateralActive, // prediction
        FeedForwardActive,
        Inactive
    }
}