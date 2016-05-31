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
    public class ByteSequencer : ISequencer<ByteSequencer>
    {
        public ByteSequencer(byte[] data, int start)
        {
            this.data = data;
            
            Reset();
            
            this.currentIndex = start;
            this.IsExceptionThrower = true;
        }
        public ByteSequencer(byte[] data)
            : this(data, 0)
        { }
        public ByteSequencer(string data)
            : this(data.GetBytes())
        { }
        private ByteSequencer(ByteSequencer bs)
        {
            this.currentIndex = bs.currentIndex;
            this.data = bs.data;
            
            this.Document = new Dictionary<string, object>();
            
            Dictionary<string, object> tempDoc = bs.Document;
            
            this.Document= Document;
            
            this.IsClosable = bs.IsClosable;
            this.IsClosed = bs.IsClosed;
        }
        
        private readonly byte[] data;
        private int currentIndex;
        
        public override bool IsEmpty
        {
            get
            {
                return currentIndex >= data.Length;
            }
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
        
        protected byte[] Slice(byte[] delimiter, bool addDelimiter)
        {
            if(delimiter.Length == 0)
                throw new ArgumentNullException("delimiter");
            
            int index = IndexOf(delimiter);
            if(index == -1)
                return null;
            
            return Slice(currentIndex, index + (addDelimiter ? delimiter.Length : 0));
        }
        
        protected byte[] Slice(int start, int end)
        {
            byte[] result = new byte[end - start];
            Array.Copy(data, start, result, 0, result.Length);
            return result;
        }
        
        public virtual ByteSequencer Reset()
        {
            this.currentIndex = 0;
            
            this.Document = new Dictionary<string, object>();
            
            this.IsClosable = true;
            this.IsClosed = false;
            
            return this;
        }
        
        public override ByteSequencer UntilAny(
            string name,
            byte[][] delimiters,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
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
            
            return Until(name, bestDelimiter, addDelimiter, converter, validator);
        }
        
        public override ByteSequencer Until(
            string name,
            byte[] delimiter,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
            validator = validator ?? (x => true);
            converter = converter ?? Identity;
            byte[] slice = Slice(delimiter, addDelimiter);
            if(slice == null)
            {
                NotFound(name);
                return this;
            }
            
            if(!validator(slice))
                throw new ValidatorException();
            
            Document[name] = converter(slice);
            currentIndex += slice.Length - (addDelimiter ? delimiter.Length : 0) +  delimiter.Length;
            
            CloseIfEnd();
            return this;
        }
        
        public override ByteSequencer RepeatUntil(
            string name,
            byte[][] delimiters,
            Func<ByteSequencer, ByteSequencer> action,
            bool addDelimiter = false,
            Func<byte[], bool> validator = null)
        {
            ThrowIfClosed();
            
            byte[] bestDelimiter = null;
            int indexbd = -1;
            foreach(byte[] bss in delimiters)
            {
                int newCurrentIndex = IndexOf(bss);
                if(indexbd == -1 || indexbd > newCurrentIndex)
                {
                    bestDelimiter = bss;
                    indexbd = newCurrentIndex;
                }
            }
            
            byte[] slice = Slice(currentIndex, indexbd);
            if(slice == null)
            {
                NotFound(name);
                return this;
            }
            
            validator = validator ?? (x => true);
            if(!validator(slice))
                throw new ValidatorException(name);
                
            currentIndex += slice.Length - (addDelimiter ? bestDelimiter.Length : 0) + bestDelimiter.Length;
            
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
            
            Document[name] = list;
            
            CloseIfEnd();
            return this;
        }
        
        public override ByteSequencer Or(string name, params Func<ByteSequencer, ByteSequencer>[] actions)
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
                        this.Document[name] = bs.Document;
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
        
        public override ByteSequencer ToEnd(string name, Func<byte[], object> converter = null)
        {
            converter = converter ?? Identity;
            
            if(this.IsClosed)
            {
                Document[name] = converter(new byte[0]);
                return this;
            }
            
            Document[name] = converter(Slice(currentIndex, data.Length));
            
            this.currentIndex = data.Length;
            
            Close();
            return this;
        }
        
        public override ByteSequencer NoGroup(byte[] value)
        {
            if(data.Length - currentIndex < value.Length)
                throw new NotFoundException("Group : " + value.GetString());
            
            byte[] slice = Slice(currentIndex, currentIndex + value.Length);
            
            for(int i = 0; i < slice.Length; ++i)
                if(slice[i] != value[i])
                    throw new NotFoundException("Group : " + value.GetString());
            
            return this;
        }
    }
}