using System.Numerics;
using System;

namespace HarkLib.Core
{
    [Serializable]
    public class UIDManager
    {
        public UIDManager(int startingValue = 0)
            : this(new BigInteger(startingValue))
        { }
        public UIDManager(BigInteger startingValue)
        {
            this.CurrentUID = startingValue - 1;
        }
        
        private readonly object mutex = new object();
        
        public BigInteger CurrentUID
        {
            get;
            private set;
        }
        
        public BigInteger Reserve()
        {
            lock(mutex)
            {
                ++CurrentUID;
                return CurrentUID;
            }
        }
        
        public void Update(BigInteger uid)
        {
            lock(mutex)
            {
                if(CurrentUID < uid)
                    CurrentUID = uid;
            }
        }
    }
}