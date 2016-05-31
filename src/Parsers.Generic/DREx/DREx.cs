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
    public class DREx<T>
         where T : ISequencer<T>
    {
        protected bool Matches(string value, string regex, out Match match)
        {
            Regex rex = new Regex(regex, RegexOptions.Singleline);
            match = rex.Match(value);
            return match.Success;
        }
        
        protected string[] ParseDelimiter(string delimiter)
        {
            return delimiter.Replace("\\!", "!").SplitNotEscaped('|').ToArray();
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
        
        protected string[] SplitDelim(string delim)
        {
            string[] sd = delim.SplitNotEscaped('!', nbMax : 2).ToArray();
            if(sd.Length == 1)
                return new string[] { sd[0], "" };
            return sd;
        }
        
        public T Eval(T obj, string value)
        {
            Match m;
            
            if(value.Length == 0)
                return obj;
            
            string type = "(?<type>((ba|sm|[isb])/)?)";
            string name = "(?<name>[a-zA-Z0-9_\\-]*)";
            string delim = "(?<delimiter>[^\\]]+)";
            
            if(Matches(value, "^\\[" + type + "" + name + ":" + delim + "\\](?<_>.*)$", out m))
            {
                string[] delims = SplitDelim(m.Groups["delimiter"].Value);
                return obj.UntilAny(
                    m.Groups["name"].Value,
                    ParseDelimiter(delims[0]),
                    converter : b => ParseType(m.Groups["type"].Value, b),
                    validator : s => delims[1].Length == 0 ? true : !s.Contains(delims[1])
                ).Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^\\[<" + name + ">\\](?<_>.*)$", out m))
            {
                obj.Document[m.Groups["name"].Value] = new List<Dictionary<string, object>>();
                
                return obj.Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^\\[" + type + "" + name + "=(?<value>[^\\]]*)\\](?<_>.*)$", out m))
            {
                obj.Document[m.Groups["name"].Value] = ParseType(m.Groups["type"].Value, m.Groups["value"].Value);
                
                return obj.Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^\\[" + type + "" + name + "\\](?<_>.*)$", out m))
            {
                obj.Document[m.Groups["name"].Value] = ParseType(m.Groups["type"].Value, new byte[0]);
                
                return obj.Eval(m.Groups["_"].Value);
            }
                
            if(Matches(value, "^\\[" + type + "\\|" + name + "\\|:" + delim + "\\](?<_>.*)$", out m))
            {
                string[] delims = SplitDelim(m.Groups["delimiter"].Value);
                return obj.UntilAny(
                    m.Groups["name"].Value,
                    ParseDelimiter(delims[0]),
                    converter : b => ParseType(m.Groups["type"].Value, b.Trim()),
                    validator : s => delims[1].Length == 0 ? true : !s.Contains(delims[1])
                ).Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^\\[\\$" + type + "\\|" + name + "\\|\\$\\](?<_>.*)$", out m))
                return obj.ToEnd(
                    m.Groups["name"].Value,
                    converter : b => ParseType(m.Groups["type"].Value, b.GetString().Trim())
                ).Eval(m.Groups["_"].Value);
            
            if(Matches(value, "^\\[\\$" + type + "" + name + "\\$\\](?<_>.*)$", out m))
                return obj.ToEnd(
                    m.Groups["name"].Value,
                    converter : b => ParseType(m.Groups["type"].Value, b)
                ).Eval(m.Groups["_"].Value);
                
            if(Matches(value, "^\\[\\<" + name + ":" + delim + "\\>\\](?<in>.*)\\[\\</\\>\\](?<_>.*)$", out m))
            {
                string[] delims = SplitDelim(m.Groups["delimiter"].Value);
                return obj.RepeatUntil(
                    m.Groups["name"].Value,
                    ParseDelimiter(delims[0]),
                    //validator : s => delims[1].Length == 0 ? true : !s.Contains(delims[1]),
                    action : x => x.Eval(m.Groups["in"].Value)
                ).Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^{(?<left>.+)\\|\\|(?<right>.+)}(?<_>.*)$", out m))
            {
                List<string> ors = new List<string>();
                string lastLeft;
                
                do
                {
                    ors.Add(m.Groups["right"].Value);
                    lastLeft = m.Groups["left"].Value;
                    
                } while(Matches(m.Groups["left"].Value, "^(?<left>.+)\\|\\|(?<right>.+)$", out m));
                
                ors.Add(lastLeft);
                ors.Reverse();
                
                var arr = ors.Select(e => (Func<T, T>)(b => b.Eval(e))).ToArray();
                
                return obj.Or(arr).Eval(m.Groups["_"].Value);
            }
            
            if(Matches(value, "^(?<pre>[^\\]]+)\\[(?<_>.*)$", out m))
                return obj.NoGroup(m.Groups["pre"].Value).Eval("[" + m.Groups["_"].Value);
            
            throw new CommandNotRecognizedException("Can't parse \"" + value + "\"");
        }
    }
}