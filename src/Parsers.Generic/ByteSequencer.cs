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
    public class ByteSequencer
    {
        public ByteSequencer(byte[] data, int start = 0)
        {
            this.data = data;
            
            Reset();
            
            this.currentIndex = start;
            this.IsExceptionThrower = true;
        }
        public ByteSequencer(string data)
            : this(data.GetBytes())
        { }
        private ByteSequencer(ByteSequencer bs)
        {
            this.currentIndex = bs.currentIndex;
            this.data = bs.data;
            
            this.Document = new Dictionary<string, object>();
            
            Dictionary<string, object> tempDoc = bs.Document;
            
            
            this.current = Document;
            
            this.IsClosable = bs.IsClosable;
            this.IsClosed = bs.IsClosed;
        }
        
        private readonly byte[] data;
        private int currentIndex;
        
        public static ByteSequencer Parse(string parser, byte[] input)
        {
            return new ByteSequencer(input).Eval(parser);
        }
        public static ByteSequencer Parse(string parser, string input)
        {
            return Parse(parser, input.GetBytes());
        }
        
        protected bool Matches(string value, string regex, out Match match)
        {
            Regex rex = new Regex(regex, RegexOptions.Singleline);
            match = rex.Match(value);
            return match.Success;
        }
        protected string[] ParseDelimiter(string delimiter)
        {
            return delimiter.Split('|');
        }
        protected object ParseType(string type, byte[] data)
        {
            if(type.Length == 0)
                return data;
            
            switch(type)
            {
                case "i/":
                    return int.Parse(data.GetString());
                    
                case "x/":
                    return Int32.Parse(data.GetString(), System.Globalization.NumberStyles.HexNumber);
                    
                case "s/":
                    return data.GetString();
                    
                case "b/":
                    return byte.Parse(data.GetString());
                    
                case "sm/":
                    return new MemoryStream(data);
                    
                case "bi/":
                    return new BigInteger(data);
                    
                case "xbi/":
                    return BigInteger.Parse(data.GetString(), NumberStyles.AllowHexSpecifier);
                    
                case "ba/":
                    return data;
                    
                default:
                    throw new UnrecognizedTypeException("Can't understand the type \"" + type + "\"");
            }
        }
        protected object ParseType(string type, string data)
        {
            if(type.Length == 0)
                return data;
            
            switch(type)
            {
                case "i/":
                    return int.Parse(data);
                    
                case "x/":
                    return Int32.Parse(data, System.Globalization.NumberStyles.HexNumber);
                    
                case "s/":
                    return data;
                    
                case "b/":
                    return byte.Parse(data);
                    
                case "sm/":
                    return new MemoryStream(data.GetBytes());
                    
                case "bi/":
                    return BigInteger.Parse(data);
                    
                case "xbi/":
                    return BigInteger.Parse(data, NumberStyles.AllowHexSpecifier);
                    
                case "ba/":
                    return data.GetBytes();
                    
                default:
                    throw new UnrecognizedTypeException("Can't understand the type \"" + type + "\"");
            }
        }
        public ByteSequencer Eval(string value)
        {
            Match m;
            
            if(value.Length == 0)
                return this;
                
            string type = "(?<type>((ba|sm|[isb])/)?)";
            string name = "(?<name>[a-zA-Z0-9_\\-]*)";
            string delim = "(?<delimiter>[^\\]]+)";
            
            if(Matches(value, "^\\[" + type + "" + name + ":" + delim + "\\](?<_>.*)$", out m))
                return UntilAny(
                    m.Groups["name"].Value,
                    ParseDelimiter(m.Groups["delimiter"].Value),
                    converter : b => ParseType(m.Groups["type"].Value, b)
                ).Eval(m.Groups["_"].Value);
                
            if(Matches(value, "^\\[" + type + "\\|" + name + "\\|:" + delim + "\\](?<_>.*)$", out m))
                return UntilAny(
                    m.Groups["name"].Value,
                    ParseDelimiter(m.Groups["delimiter"].Value),
                    converter : b => ParseType(m.Groups["type"].Value, b.Trim())
                ).Eval(m.Groups["_"].Value);
            
            if(Matches(value, "^\\[\\$" + type + "\\|" + name + "\\|\\$\\](?<_>.*)$", out m))
                return ToEnd(
                    m.Groups["name"].Value,
                    converter : b => ParseType(m.Groups["type"].Value, b.GetString().Trim())
                ).Eval(m.Groups["_"].Value);
            
            if(Matches(value, "^\\[\\$" + type + "" + name + "\\$\\](?<_>.*)$", out m))
                return ToEnd(
                    m.Groups["name"].Value,
                    converter : b => ParseType(m.Groups["type"].Value, b)
                ).Eval(m.Groups["_"].Value);
            
            if(Matches(value, "^\\[\\<" + name + ":" + delim + "\\>\\](?<in>.*)\\[\\</\\>\\](?<_>.*)$", out m))
                return RepeatUntil(
                    m.Groups["name"].Value,
                    m.Groups["delimiter"].Value,
                    action : x => x.Eval(m.Groups["in"].Value)
                ).Eval(m.Groups["_"].Value);
            
            if(Matches(value, "^{(?<left>[^|]+)\\|(?<right>.+)}(?<_>.*)$", out m))
                return Or(b => b.Eval(m.Groups["left"].Value), b => b.Eval(m.Groups["right"].Value)).Eval(m.Groups["_"].Value);
            
            throw new CommandNotRecognizedException("Can't parse \"" + value + "\"");
        }
        
        public Dictionary<string, object> Document
        {
            get;
            private set;
        }
        public Dictionary<string, object> current
        {
            get;
            private set;
        }
        
        public bool IsClosed
        {
            get;
            private set;
        }
        public bool IsClosable
        {
            get;
            private set;
        }
        public bool IsExceptionThrower
        {
            get;
            private set;
        }
        
        public virtual ByteSequencer Reset()
        {
            this.currentIndex = 0;
            
            this.Document = new Dictionary<string, object>();
            this.current = Document;
            
            this.IsClosable = true;
            this.IsClosed = false;
            
            return this;
        }
        
        public ByteSequencer ToClosable(bool isClosable)
        {
            this.IsClosable = isClosable;
            this.IsClosed = false;
            
            return this;
        }
        
        public ByteSequencer ToExceptionThrower(bool isExceptionThrower)
        {
            this.IsExceptionThrower = isExceptionThrower;
            return this;
        }
        
        protected static Func<byte[], object> Identity = x => x;
        
        public void ThrowIfClosed()
        {
            if(IsClosed)
                throw new ClosedSequencerException();
        }
        
        protected void CloseIfEnd()
        {
            if(!IsClosable)
                return;
            
            if(currentIndex >= data.Length)
                IsClosed = true;
        }
        
        protected byte[] Slice(int start, int end)
        {
            byte[] result = new byte[end - start];
            Array.Copy(data, start, result, 0, result.Length);
            return result;
        }
        
        public ByteSequencer ThrowIfEmpty()
        {
            if(data.Length <= currentIndex)
                NotFound("The sequence is empty.");
            
            return this;
        }
        public ByteSequencer ThrowIfNotEmpty()
        {
            if(data.Length > currentIndex)
                NotFound("The sequence is empty.");
            
            return this;
        }
        
        public ByteSequencer Until(
            string name,
            byte delimiter,
            bool skipDelimiter = true,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            int index = IndexOf(new byte[] { delimiter });
            
            if(index == -1)
            {
                NotFound(name);
                return this;
            }
            
            validator = validator ?? (x => true);
            converter = converter ?? Identity;
            byte[] slice = Slice(currentIndex, index + (addDelimiter ? 1 : 0));
            if(!validator(slice))
                throw new ValidatorException();
            current[name] = converter(slice);
            currentIndex = index + (skipDelimiter ? 1 : 0);
            
            CloseIfEnd();
            return this;
        }
        
        protected void NotFound(string name, object value = null)
        {
            Close();
            
            if(IsExceptionThrower)
                throw new NotFoundException("Can't match \"" + name + "\".");
            
            current[name] = value ?? new byte[0];
        }
        
        protected int IndexOf(byte[] delimiter)
        {
            if(delimiter.Length == 0)
                return currentIndex;
                
            if(delimiter.Length == 1)
            {
                if(delimiter[0] == '$')
                    return data.Length;
                else
                    return Array.IndexOf(data, delimiter[0], currentIndex);
            }
            
            int index = currentIndex - 1;
            
            while(true)
            {
                index = Array.IndexOf(data, delimiter[0], index + 1);
                
                if(index == -1)
                    return -1;
                
                bool found = true;
                for(int i = 1; i < delimiter.Length; ++i)
                    if(index + i >= data.Length || data[index + i] != delimiter[i])
                    {
                        found = false;
                        break;
                    }
                
                if(found)
                    break;
            }
            
            return index;
        }
        
        protected byte[] Slice(byte[] delimiter, bool skipDelimiter, bool addDelimiter)
        {
            if(delimiter.Length == 0)
                throw new ArgumentNullException("delimiter");
            
            int index = IndexOf(delimiter);
            if(index == -1)
                return null;
            
            return Slice(currentIndex, index + (addDelimiter ? delimiter.Length : 0));
        }
        
        public ByteSequencer UntilAny(string name, byte[][] delimiters, bool skipDelimiter = true, bool addDelimiter = false, Func<byte[], object> converter = null, Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            if(delimiters.Length == 1) // Trivial case
                return Until(name, delimiters[0], skipDelimiter, addDelimiter, converter, validator);
            
            byte[] bestDelimiter = null;
            int index = -1;
            foreach(byte[] bs in delimiters)
            {
                int currentIndex = IndexOf(bs);
                if(index == -1 || index > currentIndex)
                {
                    bestDelimiter = bs;
                    index = currentIndex;
                }
            }
            
            return Until(name, bestDelimiter, skipDelimiter, addDelimiter, converter, validator);
        }
        public ByteSequencer Until(string name, byte[] delimiter, bool skipDelimiter = true, bool addDelimiter = false, Func<byte[], object> converter = null, Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            if(delimiter.Length == 1) // Trivial case
                return Until(name, delimiter[0], skipDelimiter, addDelimiter, converter, validator);
            
            validator = validator ?? (x => true);
            converter = converter ?? Identity;
            byte[] slice = Slice(delimiter, skipDelimiter, addDelimiter);
            if(slice == null)
            {
                NotFound(name);
                return this;
            }
            
            if(!validator(slice))
                throw new ValidatorException();
            
            current[name] = converter(slice);
            currentIndex += slice.Length - (addDelimiter ? delimiter.Length : 0) + (skipDelimiter ? delimiter.Length : 0);
            
            CloseIfEnd();
            return this;
        }
        public ByteSequencer Until(
            string name,
            char delimiter,
            bool skipDelimiter = true,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            return Until(
                name,
                (byte)delimiter,
                skipDelimiter,
                addDelimiter,
                converter,
                validator);
        }
        public ByteSequencer Until(
            string name,
            string delimiter,
            bool skipDelimiter = true,
            bool addDelimiter = false,
            Func<string, object> converter = null,
            Func<string, bool> validator = null)
        {
            validator = validator ?? (x => true);
            converter = converter ?? (x => x);
            Func<byte[], bool> v = (bs => validator(bs.GetString()));
            
            return Until(
                name,
                delimiter.GetBytes(),
                skipDelimiter,
                addDelimiter,
                (bs => converter(bs.GetString())),
                validator : v
            );
        }
        public ByteSequencer UntilAny(
            string name,
            string[] delimiters,
            bool skipDelimiter = true,
            bool addDelimiter = false,
            Func<string, object> converter = null,
            Func<string, bool> validator = null)
        {
            validator = validator ?? (x => true);
            converter = converter ?? (x => x);
            Func<byte[], bool> v = (bs => validator(bs.GetString()));
            
            return UntilAny(
                name,
                delimiters.Select(d => d.GetBytes()).ToArray(),
                skipDelimiter,
                addDelimiter,
                (bs => converter(bs.GetString())),
                validator : v
            );
        }
        
        public ByteSequencer RepeatUntil(string name, byte delimiter, Func<ByteSequencer, ByteSequencer> action, bool skipDelimiter = true, bool addDelimiter = false)
        {
            // TODO : To change
            int index = currentIndex;
            while(data[index] != delimiter)
                ++index;
            byte[] subData = Slice(currentIndex, index + (addDelimiter ? 1 : 0));
            ByteSequencer bs = new ByteSequencer(subData);
            currentIndex = index + (skipDelimiter ? 1 : 0);
            
            while(!bs.IsClosed)
                action(bs);
            
            if(String.IsNullOrEmpty(name))
            {
                foreach(var e in bs.Document)
                    current.Add(e.Key, e.Value);
            }
            else
                current[name] = bs.Document;
            
            return this;
        }
        public ByteSequencer RepeatUntil(string name, byte[] delimiter, Func<ByteSequencer, ByteSequencer> action, bool skipDelimiter = true, bool addDelimiter = false)
        {
            ThrowIfClosed();
            
            byte[] slice = Slice(delimiter, skipDelimiter, addDelimiter);
            if(slice == null)
            {
                NotFound(name);
                return this;
            }
            currentIndex += slice.Length - (addDelimiter ? delimiter.Length : 0) + (skipDelimiter ? delimiter.Length : 0);
            
            ByteSequencer bs;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            int index = 0;
            do
            {
                bs = new ByteSequencer(slice, index);
                action(bs);
                if(index == bs.currentIndex)
                    break;
                list.Add(bs.Document);
                
                index = bs.currentIndex;
                
            } while(!bs.IsClosed);
            
            current[name] = list;
            
            CloseIfEnd();
            return this;
        }
        public ByteSequencer RepeatUntil(string name, char delimiter, Func<ByteSequencer, ByteSequencer> action, bool skipDelimiter = true, bool addDelimiter = false)
        {
            return RepeatUntil(name, (byte)delimiter, action, skipDelimiter, addDelimiter);
        }
        public ByteSequencer RepeatUntil(string name, string delimiter, Func<ByteSequencer, ByteSequencer> action, bool skipDelimiter = true, bool addDelimiter = false)
        {
            return RepeatUntil(name, delimiter.GetBytes(), action, skipDelimiter, addDelimiter);
        }
        
        public ByteSequencer Wrap(string name, Func<ByteSequencer, ByteSequencer> action)
        {
            Dictionary<string, object> newCurrent = new Dictionary<string, object>();
            Dictionary<string, object> oldCurrent = current;
            
            current[name] = newCurrent;
            
            action(this);
            
            current = oldCurrent;
            
            return this;
        }
        
        public ByteSequencer Or(string name, params Func<ByteSequencer, ByteSequencer>[] actions)
        {
            bool found = false;
            
            foreach(var action in actions)
                try
                {
                    ByteSequencer bs = new ByteSequencer(data, currentIndex);
                    action(bs);
                    
                    if(String.IsNullOrEmpty(name))
                        this.Document = bs.Document;
                    else
                        this.current[name] = bs.Document;
                    this.currentIndex = bs.currentIndex;
                    found = true;
                    
                    break;
                }
                catch(NotFoundException)
                { }
                catch(ClosedSequencerException)
                { }
                catch(ValidatorException)
                { }
            
            if(!found)
                NotFound("Or");
            
            CloseIfEnd();
            return this;
        }
        public ByteSequencer Or(params Func<ByteSequencer, ByteSequencer>[] actions)
        {
            return Or(null, actions);
        }
        
        public ByteSequencer UntilAny(string name, byte[] delimiters, bool skipDelimiter = true, bool addDelimiter = false, Func<byte[], object> converter = null)
        {
            // TODO : to change
            ThrowIfClosed();
            converter = converter ?? Identity;
            List<byte> value = new List<byte>();
            List<byte> lDelimiters = new List<byte>(delimiters);
            
            while(data.Length > currentIndex && lDelimiters.Contains(data[currentIndex]))
            {
                value.Add(data[currentIndex]);
                ++currentIndex;
            }
            
            if(addDelimiter)
                value.Add(data[currentIndex]);
            
            current[name] = converter(value.ToArray());
            
            if(skipDelimiter)
                ++currentIndex;
            
            CloseIfEnd();
            return this;
        }
        
        public ByteSequencer ToEnd(string name, Func<byte[], object> converter = null)
        {
            ThrowIfClosed();
            converter = converter ?? Identity;
            current[name] = converter(Slice(currentIndex, data.Length));
            
            this.currentIndex = data.Length;
            
            Close();
            return this;
        }
        
        public ParserResult Close()
        {
            this.IsClosed = true;
            return new ParserResult(this.Document);
        }
    }
}