using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Location
    {
        public int X
        {
            get;
            set;
        }
        
        public int Y
        {
            get;
            set;
        }
        
        public double Distance(Location location)
        {
            int d_x = X - location.X;
            int d_y = Y - location.Y;
            
            return Math.Sqrt(d_x * d_x + d_y * d_y);
        }
    }
}