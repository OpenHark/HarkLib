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
        
        private readonly Stream stream;
        
        private bool EndOfStream = false;
        
        public override bool IsEmpty
        {
            get
            {
                return EndOfStream;
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
        
        protected byte[] Slice(
            string name,
            byte[][] ends,
            bool addDelimiter = false)
        {
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
                        if(value == ends[i][endIndexes[i]])
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
                    NotFound(name);
                }
                
                byte[] result = ms.ToArray();
                if(!addDelimiter)
                {
                    byte[] newResult = new byte[result.Length - ends[lastFound].Length];
                    Array.Copy(result, 0, newResult, 0, newResult.Length);
                    result = newResult;
                }
                
                return result;
            }
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
                do
                {
                    ss = new StreamSequencer(ms);
                    action(ss);
                    data.Add(ss.Document);
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
        
        
        public StreamSequencer NoGroup(byte[] value)
        {
            for(int i = 0; i < value.Length; ++i)
            {
                if(stream.ReadByte() != (int)value[i])
                    throw new NotFoundException();
            }
            
            return this;
        }
        public override StreamSequencer NoGroup(string value)
        {
            return NoGroup(value.ToCharArray().Cast<byte>().ToArray());
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        public static implicit operator ParserResult(StreamSequencer bs)
        {
            return new ParserResult(bs.Document);
        }
        
        public static StreamSequencer operator | (StreamSequencer bs, string eval)
        {
            return bs.Eval(eval);
        }
        
        public static StreamSequencer Parse(string parser, Stream input)
        {
            return new StreamSequencer(input).Eval(parser);
        }
        public static StreamSequencer Parse(string parser, byte[] input)
        {
            return Parse(parser, new MemoryStream(input));
        }
        public static StreamSequencer Parse(string parser, string input)
        {
            return Parse(parser, input.GetBytes());
        }
        
        public static bool TryParse(string parser, Stream input, out StreamSequencer result)
        {
            try
            {
                result = new StreamSequencer(input).Eval(parser);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public static bool TryParse(string parser, byte[] input, out StreamSequencer result)
        {
            return TryParse(parser, new MemoryStream(input), out result);
        }
        public static bool TryParse(string parser, string input, out StreamSequencer result)
        {
            return TryParse(parser, input.GetBytes(), out result);
        }
        
        public static readonly DREx<StreamSequencer> DREx = new DREx<StreamSequencer>();
        
        public override StreamSequencer Eval(string drex)
        {
            return DREx.Eval(this, drex);
        }
        
        public override StreamSequencer Or(string name, params Func<StreamSequencer, StreamSequencer>[] actions)
        {
            throw new NotImplementedException();
        }
    }
}