using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Region
    {
        public DendriteSegment Segment
        {
            get;
            set;
        }
        
        public double Boost
        {
            get;
            set;
        }
        
        public double InhibitionRadius
        {
            get;
            set;
        }
    }
}