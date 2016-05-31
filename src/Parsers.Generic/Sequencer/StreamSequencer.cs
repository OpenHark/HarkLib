using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.IO;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    public class StreamSequencer : ISequencer<StreamSequencer>
    {
        public StreamSequencer(string value)
            : this(value.GetBytes())
        { }
        public StreamSequencer(byte[] data)
            : this(new MemoryStream(data))
        { }
        public StreamSequencer(Stream stream)
        {
            this.stream = stream;
            
            this.Document = new Dictionary<string, object>();
        }
        
        private bool EndOfStream = false;
        private Stream stream;
        
        public override bool IsEmpty
        {
            get
            {
                return EndOfStream;
            }
        }
        
        protected byte[] Slice(
            string name,
            byte[][] ends,
            bool addDelimiter = false)
        {
            for(int i = 0; i < ends.Length; ++i)
                if(ends[i].Length == 1 && ends[i][0] == '$')
                    ends[i] = null;
            
            using(MemoryStream ms = new MemoryStream())
            {
                int value = -1;
                int[] endIndexes = new int[ends.Length];
                int lastFound = -1;
                bool exit = false;
                while(!exit && (value = stream.ReadByte()) != -1)
                {
                    lastFound = -1;
                    for(int i = 0; i < ends.Length; ++i)
                    {
                        if(ends[i] == null)
                            lastFound = i;
                        else if(value == ends[i][endIndexes[i]])
                        {
                            ++endIndexes[i];
                            lastFound = i;
                            if(endIndexes[i] == ends[i].Length)
                            {
                                exit = true;
                                break;
                            }
                        }
                    }
                        
                    if(lastFound == -1)
                    {
                        for(int i = 0; i < endIndexes.Length; ++i)
                            endIndexes[i] = 0;
                    }
                    
                    ms.WriteByte((byte)value);
                }
                
                if(value == -1)
                {
                    EndOfStream = true;
                    if(lastFound == -1 || ends[lastFound] != null)
                        NotFound(name);
                }
                
                byte[] result = ms.ToArray();
                if(!addDelimiter && ends[lastFound] != null)
                {
                    byte[] newResult = new byte[result.Length - ends[lastFound].Length];
                    Array.Copy(result, 0, newResult, 0, newResult.Length);
                    result = newResult;
                }
                
                return result;
            }
        }
        
        public override StreamSequencer Until(
            string name,
            byte[] end,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
            if(end.Length == 0) // Trivial case
            {
                Document[name] = new byte[0];
                return this;
            }
            
            converter = converter ?? Identity;
            validator = validator ?? (x => true);
            
            using(MemoryStream ms = new MemoryStream())
            {
                int value;
                int endIndex = 0;
                while((value = stream.ReadByte()) != -1)
                {
                    if(value == end[endIndex])
                    {
                        ++endIndex;
                        if(endIndex == end.Length)
                            break;
                    }
                    else
                    {
                        if(endIndex != 0)
                        {
                            for(int i = 0; i < endIndex; ++i)
                                ms.WriteByte(end[i]);
                            endIndex = 0;
                            
                            if(value == end[0])
                            {
                                ++endIndex;
                                continue;
                            }
                        }
                        
                        ms.WriteByte((byte)value);
                    }
                }
                
                if(value == -1)
                {
                    EndOfStream = true;
                    NotFound(name);
                }
                
                if(addDelimiter)
                    ms.Write(end, 0, end.Length);
                
                byte[] result = ms.ToArray();
                if(!validator(result))
                    throw new ValidatorException(name);
                Document[name] = converter(result);
            }
            
            CloseIfEnd();
            return this;
        }
        
        public override StreamSequencer UntilAny(
            string name,
            byte[][] ends,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
            if(ends.Length == 0) // Trivial case
            {
                Document[name] = new byte[0];
                return this;
            }
            
            converter = converter ?? Identity;
            validator = validator ?? (x => true);
            
            byte[] result = Slice(name, ends, addDelimiter);
            
            if(!validator(result))
                throw new ValidatorException(name);
            Document[name] = converter(result);
            
            CloseIfEnd();
            return this;
        }
        
        public override StreamSequencer RepeatUntil(
            string name,
            byte[][] delimiters,
            Func<StreamSequencer, StreamSequencer> action,
            bool addDelimiter = false,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
            byte[] result = Slice(name, delimiters, addDelimiter);
            
            validator = validator ?? (x => true);
            if(!validator(result))
                throw new ValidatorException(name);
            
            using(MemoryStream ms = new MemoryStream(result))
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                
                StreamSequencer ss;
                Stream currentStream = ms;
                do
                {
                    ss = new StreamSequencer(currentStream);
                    action(ss);
                    data.Add(ss.Document);
                    currentStream = ss.stream;
                } while(!ss.IsClosed);
                
                Document[name] = data;
            }
            
            CloseIfEnd();
            return this;
        }
        
        public override StreamSequencer ToEnd(string name, Func<byte[], object> converter = null)
        {
            converter = converter ?? Identity;
            
            if(this.IsClosed)
            {
                Document[name] = converter(new byte[0]);
                return this;
            }
            
            using(MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Document[name] = converter(ms.ToArray());
            }
            
            EndOfStream = true;
            
            CloseIfEnd();
            Close();
            return this;
        }
        
        public override StreamSequencer NoGroup(byte[] value)
        {
            for(int i = 0; i < value.Length; ++i)
            {
                if(stream.ReadByte() != (int)value[i])
                    throw new NotFoundException("Group : " + value.GetString());
            }
            
            return this;
        }
        
        public override StreamSequencer Or(string name, params Func<StreamSequencer, StreamSequencer>[] actions)
        {
            SplitStream[] streams = stream.Split(actions.Length);
            
            for(int i = 0; i < actions.Length; ++i)
                try
                {
                    StreamSequencer ss = new StreamSequencer(streams[i]);
                    actions[i](ss);
                    
                    if(String.IsNullOrEmpty(name))
                        this.Document = ss.Document;
                    else
                        this.Document[name] = ss.Document;
                    
                    this.stream = streams[i];
                    
                    streams[i].Select();
                    EndOfStream = streams[i].IsEndOfStream;
                    
                    CloseIfEnd();
                    return this;
                }
                catch(NotFoundException)
                { }
                catch(ClosedSequencerException)
                { }
                catch(ValidatorException)
                { }
            
            NotFound("Or");
            CloseIfEnd();
            return this;
        }
    }
}