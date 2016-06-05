using System.Runtime.InteropServices;
using System;

namespace HarkLib.Core
{
    public class Processor
    {
        [DllImport("kernel32.dll")]
        private extern static int QueryPerformanceCounter(ref long x);
        
        [DllImport("kernel32.dll")]
        private extern static int QueryPerformanceFrequency(ref long x);
        
        public static long ProcessorCounter
        {
            get
            {
                long value = 0;
                QueryPerformanceCounter(ref value);
                return value;
            }
        }
        
        public static long ProcessorFrequency
        {
            get
            {
                long value = 0;
                QueryPerformanceFrequency(ref value);
                return value;
            }
        }
        
        public static Random CreateRandom()
        {
            return new Random((int)ProcessorCounter);
        }
    }
}