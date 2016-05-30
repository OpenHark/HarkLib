using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Core
{
    public class StreamSplit
    {
        public StreamSplit(Stream stream, int nbSplit = 2)
        {
            this.Streams = new Stream[nbSplit];
            this.Counts = new int[nbSplit];
            this.Values = new List<byte>();
            this.EndOfStream = false;
            this.BaseStream = stream;
            this.NbSplit = nbSplit;
            this.ReadMin = 0;
            
            for(int i = 0; i < nbSplit; ++i)
                this.Streams[i] = new SplitedStream(stream, this, i);
        }
        
        public Stream BaseStream
        {
            get;
            private set;
        }
        
        public Stream[] Streams
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
        
        protected int ReadByte(int id)
        {
            bool isLast = false;
            try
            {
                int count = Counts[id];
                
                if(ReadMin == 0)
                    isLast = Counts.Where((v,i) => i != id).All(v => v > count);
                else
                    isLast = count == ReadMin;
                
                if(Values.Count == 0 || count - ReadMin >= Values.Count)
                {
                    if(EndOfStream)
                        return -1;
                    
                    int value = BaseStream.ReadByte();
                    
                    if(value == -1)
                        EndOfStream = true;
                    
                    Values.Add((byte)value);
                    return value;
                }
                else
                    return Values[count - ReadMin];
            }
            finally
            {
                if(isLast)
                {
                    ++ReadMin;
                    Values.RemoveAt(0);
                }
                ++Counts[id];
            }
        }
        protected int Read(int id, byte[] data, int start, int length)
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
        
        public class SplitedStream : BasedStream
        {
            public SplitedStream(Stream stream, StreamSplit splitter, int id)
                : base(stream)
            {
                this.StreamSplit = splitter;
                this.ID = id;
            }
            
            public int ID
            {
                get;
                private set;
            }
            
            public StreamSplit StreamSplit
            {
                get;
                private set;
            }
            
            public override int Read(byte[] data, int start, int length)
            {
                return StreamSplit.Read(ID, data, start, length);
            }
            
            public override void Write(byte[] data, int start, int length)
            {
                BaseStream.Write(data, start, length);
            }
        }
    }
}