using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public interface IInput
    {
        bool Value
        {
            get;
        }
        
        Location Location
        {
            get;
        }
    }
}