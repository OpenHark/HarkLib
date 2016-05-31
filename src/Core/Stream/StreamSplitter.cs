using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Core
{
    public class StreamSplitter
    {
        public StreamSplitter(Stream stream, int nbSplit = 2)
        {
            this.Validities = new bool[nbSplit];
            this.Streams = new SplitStream[nbSplit];
            this.Counts = new int[nbSplit];
            this.Values = new List<byte>();
            this.EndOfStream = false;
            this.BaseStream = stream;
            this.NbSplit = nbSplit;
            this.ReadMin = 0;
            
            for(int i = 0; i < nbSplit; ++i)
            {
                this.Streams[i] = new SplitStream(stream, this, i);
                this.Validities[i] = true;
                this.Counts[i] = 0;
            }
        }
        
        public Stream BaseStream
        {
            get;
            private set;
        }
        
        public SplitStream[] Streams
        {
            get;
            private set;
        }
        
        public int NbSplit
        {
            get;
            private set;
        }
        
        protected List<byte> Values
        {
            get;
            private set;
        }
        
        protected int[] Counts
        {
            get;
            private set;
        }
        protected int ReadMin
        {
            get;
            private set;
        }
        
        public bool EndOfStream
        {
            get;
            private set;
        }
        
        public bool[] Validities
        {
            get;
            private set;
        }
        
        protected bool IsLast(int id)
        {
            int count = Counts[id];
            
            return Counts
                .Select((v,i) => new { id = i, value = v})
                .Where(x => Validities[x.id])
                .Where(x => x.id != id)
                .All(x => x.value > count);
        }
        
        protected int ReadByte(int id)
        {
            int count = Counts[id];
            
            int resultingValue;
            if(Values.Count == 0 || count - ReadMin >= Values.Count)
            {
                if(EndOfStream)
                    return -1;
                
                int value = BaseStream.ReadByte();
                
                if(value == -1)
                    EndOfStream = true;
                else
                    Values.Add((byte)value);
                
                resultingValue = value;
            }
            else
                resultingValue = Values[count - ReadMin];
            
            if(IsLast(id))
            {
                ++ReadMin;
                Values.RemoveAt(0);
            }
            ++Counts[id];
            
            return resultingValue;
        }
        
        protected internal int Read(int id, byte[] data, int start, int length)
        {
            for(int i = 0; i < length; ++i)
            {
                int value = ReadByte(id);
                if(value == -1)
                    return i;
                data[start + i] = (byte)value;
            }
            
            return length;
        }
        
        public void Disable(int id)
        {
            while(true)
            {
                int count = Counts[id];
                
                if(IsLast(id))
                {
                    ++ReadMin;
                    Values.RemoveAt(0);
                    ++Counts[id];
                }
                else
                {
                    break;
                }
            }
            
            Validities[id] = false;
        }
        
        public void Select(int id)
        {
            for(int i = 0; i < NbSplit; ++i)
                if(i != id && !Validities[i])
                    Disable(i);
        }
        
        public bool IsEndOfStream(int id)
        {
            return EndOfStream && Counts[id] - ReadMin == Values.Count;
        }
    }
}