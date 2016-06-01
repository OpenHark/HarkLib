using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI
{
    public class ReverseGenomeComparer<T> : IComparer<Genome<T>>
    {
       public int Compare(Genome<T> x, Genome<T> y)  
       {
           return x.Fitness.CompareTo(y.Fitness);
       }
    }
}