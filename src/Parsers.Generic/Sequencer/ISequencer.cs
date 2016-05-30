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
    public abstract class ISequencer<T>
        where T : ISequencer<T>
    {
        public ISequencer()
        {
            Document = new Dictionary<string, object>();
        }
        
        protected static Func<byte[], object> Identity = x => x;
        
        public Dictionary<string, object> Document
        {
            get;
            protected set;
        }
        
        public bool IsClosed
        {
            get;
            protected set;
        }
        
        public bool IsClosable
        {
            get;
            protected set;
        }
        
        public bool IsExceptionThrower
        {
            get;
            protected set;
        }
        
        public T ToClosable(bool isClosable)
        {
            this.IsClosable = isClosable;
            
            if(!isClosable)
                this.IsClosed = false;
            
            return (T)this;
        }
        
        public T ToExceptionThrower(bool isExceptionThrower)
        {
            this.IsExceptionThrower = isExceptionThrower;
            return (T)this;
        }
        
        public T ThrowIfClosed()
        {
            if(IsClosed)
                throw new ClosedSequencerException();
            return (T)this;
        }
        
        protected void CloseIfEnd()
        {
            if(!IsClosable)
                return;
            
            if(IsEmpty)
                IsClosed = true;
        }
        
        public abstract bool IsEmpty
        {
            get;
        }
        
        public T ThrowIfEmpty()
        {
            if(IsEmpty)
                NotFound("The sequence is empty.");
            
            return (T)this;
        }
        
        public T ThrowIfNotEmpty()
        {
            if(!IsEmpty)
                NotFound("The sequence is not empty.");
            
            return (T)this;
        }
        
        
        protected void NotFound(string name, object value = null)
        {
            Close();
            
            if(IsExceptionThrower)
                throw new NotFoundException("Can't match \"" + name + "\".");
            
            Document[name] = value ?? new byte[0];
        }
        
        public abstract T UntilAny(
            string name,
            byte[][] delimiters,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null);
        public T UntilAny(
            string name,
            byte[] delimiters,
            bool addDelimiter = false,
            Func<byte[], object> converter = null)
        {
            return UntilAny(
                name,
                delimiters.Select(b => new byte[] { b }).ToArray(),
                addDelimiter,
                converter);
        }
        public T UntilAny(
            string name,
            string[] delimiters,
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
                addDelimiter,
                (bs => converter(bs.GetString())),
                validator : v
            );
        }
        
        
        
        
            
        public T Until(
            string name,
            byte delimiter,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            return Until(
                name,
                new byte[] { delimiter },
                addDelimiter,
                converter,
                validator);
        }
        public abstract T Until(
            string name,
            byte[] delimiter,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null);
        public T Until(
            string name,
            char delimiter,
            bool addDelimiter = false,
            Func<byte[], object> converter = null,
            Func<byte[], bool> validator = null)
        {
            return Until(
                name,
                (byte)delimiter,
                addDelimiter,
                converter,
                validator);
        }
        public T Until(
            string name,
            string delimiter,
            bool addDelimiter = false,
            Func<string, object> converter = null,
            Func<string, bool> validator = null)
        {
            validator = validator ?? (x => true);
            converter = converter ?? (x => x);
            
            return Until(
                name,
                delimiter.GetBytes(),
                addDelimiter,
                bs => converter(bs.GetString()),
                validator : bs => validator(bs.GetString())
            );
        }
        
        public abstract T RepeatUntil(
            string name,
            byte[][] delimiters,
            Func<T, T> action,
            bool addDelimiter = false,
            Func<byte[], bool> validator = null);
        public T RepeatUntil(
            string name,
            byte delimiter,
            Func<T, T> action,
            bool addDelimiter = false,
            Func<byte[], bool> validator = null)
        {
            return RepeatUntil(
                name,
                new byte[][] { new byte[] { delimiter } },
                action,
                addDelimiter,
                validator);
        }
        public T RepeatUntil(
            string name,
            char delimiter,
            Func<T, T> action,
            bool addDelimiter = false,
            Func<string, bool> validator = null)
        {
            validator = validator ?? (x => true);
            
            return RepeatUntil(
                name,
                (byte)delimiter,
                action,
                addDelimiter,
                validator : b => validator(b.GetString()));
        }
        public T RepeatUntil(
            string name,
            string delimiter,
            Func<T, T> action,
            bool addDelimiter = false,
            Func<string, bool> validator = null)
        {
            return RepeatUntil(
                name,
                new string[] { delimiter },
                action,
                addDelimiter,
                validator);
        }
        public T RepeatUntil(
            string name,
            string[] delimiters,
            Func<T, T> action,
            bool addDelimiter = false,
            Func<string, bool> validator = null)
        {
            validator = validator ?? (x => true);
            
            return RepeatUntil(
                name,
                delimiters.Select(s => s.GetBytes()).ToArray(),
                action,
                addDelimiter,
                validator : b => validator(b.GetString()));
        }
        
        public T Wrap(string name, Func<T, T> action)
        {
            Dictionary<string, object> newCurrent = new Dictionary<string, object>();
            Dictionary<string, object> oldCurrent = Document;
            
            Document[name] = newCurrent;
            
            action((T)this);
            
            Document = oldCurrent;
            
            return (T)this;
        }
        
        public abstract T Or(string name, params Func<T, T>[] actions);
        public T Or(params Func<T, T>[] actions)
        {
            return Or(null, actions);
        }
        
        
        public abstract T ToEnd(
            string name,
            Func<byte[], object> converter = null);
        
        public ParserResult Close()
        {
            this.IsClosed = true;
            return new ParserResult(this.Document);
        }
        
        public abstract T NoGroup(string value);
        
        public abstract T Eval(string drex);
    }
}