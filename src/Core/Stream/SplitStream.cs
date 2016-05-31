using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Core
{
    public class SplitStream : BasedStream
    {
        public SplitStream(Stream stream, StreamSplitter splitter, int id)
            : base(stream)
        {
            this.StreamSplitter = splitter;
            this.ID = id;
        }
        
        public int ID
        {
            get;
            private set;
        }
        
        public StreamSplitter StreamSplitter
        {
            get;
            private set;
        }
        
        public bool IsEndOfStream
        {
            get
            {
                return StreamSplitter.IsEndOfStream(ID);
            }
        }
        
        public override int Read(byte[] data, int start, int length)
        {
            return StreamSplitter.Read(ID, data, start, length);
        }
        
        public override void Write(byte[] data, int start, int length)
        {
            BaseStream.Write(data, start, length);
        }
        
        public void Disable()
        {
            StreamSplitter.Disable(ID);
        }
        
        public void Select()
        {
            StreamSplitter.Select(ID);
        }
    }
}