using System.IO;
using System;

namespace HarkLib.Core
{
    public abstract class BasedStream : Stream
    {
        public BasedStream(Stream stream)
        {
            this.BaseStream = stream;
        }
        
        public Stream BaseStream
        {
            get;
            private set;
        }
        
        public override bool CanRead
        {
            get
            {
                return BaseStream.CanRead;
            }
        }
        
        public override bool CanSeek
        {
            get
            {
                return BaseStream.CanSeek;
            }
        }
        
        public override bool CanWrite
        {
            get
            {
                return BaseStream.CanWrite;
            }
        }
        
        public override long Length
        {
            get
            {
                return BaseStream.Length;
            }
        }
        
        public override long Position
        {
            get
            {
                return BaseStream.Position;
            }
            set
            {
                BaseStream.Position = value;
            }
        }
        
        public override int ReadTimeout
        {
            get
            {
                return BaseStream.ReadTimeout;
            }
            set
            {
                BaseStream.ReadTimeout = value;
            }
        }
        
        public override int WriteTimeout
        {
            get
            {
                return BaseStream.WriteTimeout;
            }
            set
            {
                BaseStream.WriteTimeout = value;
            }
        }
        
        public override void Flush()
        {
            BaseStream.Flush();
        }
        
        public override long Seek(long position, SeekOrigin origin)
        {
            return BaseStream.Seek(position, origin);
        }
        
        public override void SetLength(long length)
        {
            BaseStream.SetLength(length);
        }
    }
}