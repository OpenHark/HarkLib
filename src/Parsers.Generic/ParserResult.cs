using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    using Dico = Dictionary<string, object>;
    using ListDico = List<Dictionary<string, object>>;
    
    public class ParserResult
    {
        public ParserResult(Dico data)
        {
            this.Dictionary = data;
        }
        
        public Dico Dictionary
        {
            get;
            private set;
        }
        
        protected static object find(
            string[] names,
            int index,
            Dico current)
        {
            if(names.Length <= index)
                throw new NotFoundException();
            
            string currentName = names[index];
            
            if(!current.ContainsKey(currentName))
                throw new NotFoundException();
            
            object obj = current[currentName];
            
            ++index;
            if(names.Length == index)
                return obj;
            
            currentName = names[index];
            
            Dico submap;
            
            if(currentName.StartsWith("<") && currentName.EndsWith(">"))
            {
                currentName = currentName.Substring(1, currentName.Length - 2);
                var list = obj as ListDico;
                
                if(currentName.Contains("="))
                {
                    bool caseInsensitive = currentName.StartsWith("|") && currentName.EndsWith("|");
                    if(caseInsensitive)
                        currentName = currentName.Substring(1, currentName.Length - 2);
                    
                    string[] sp = currentName.Split(new char[] { '=' }, 2);
                    string key = sp[0];
                    string value = sp[1];
                    
                    Func<string, string, bool> comparator;
                    if(caseInsensitive)
                        comparator = (a,b) => a.ToLower() == b.ToLower();
                    else
                        comparator = (a,b) => a == b;
                    
                    try
                    {
                        submap = list
                            .Where(e => comparator(e[key] as string, value))
                            .First();
                    }
                    catch
                    {
                        throw new NotFoundException();
                    }
                }
                else
                {
                    int id = int.Parse(currentName);
                    submap = list[id];
                }
                
                ++index;
                if(names.Length == index)
                    return submap;
            }
            else
            {
                submap = obj as Dico;
                
                if(submap == null)
                    throw new NotFoundException();
            }
            
            return find(names, index, submap);
        }
        public object this[string path]
        {
            get
            {
                return find(path.Split('.'), 0, Dictionary);
            }
        }
        
        public ListDico GetList(string path)
        {
            return this[path] as ListDico;
        }
        
        public byte[] GetBytes(string path)
        {
            return this[path] as byte[];
        }
        
        public string GetString(string path)
        {
            return this[path] as string;
        }
        
        public Dico GetMap(string path)
        {
            return this[path] as Dico;
        }
        
        public Dictionary<string, List<object>> GetReverseMap(string listPath)
        {
            var list = GetList(listPath);
            
            Dictionary<string, List<object>> result = new Dictionary<string, List<object>>();
            
            foreach(Dico l in list)
            {
                foreach(var e in l)
                {
                    if(result.ContainsKey(e.Key))
                        result.Add(e.Key, new List<object>());
                    result[e.Key].Add(e.Value);
                }
            }
            
            return result;
        }
        
        public T Get<T>(string path)
        {
            return (T)this[path];
        }
        
        public bool Exists(string path)
        {
            try
            {
                object _ = this[path];
                return true;
            }
            catch(NotFoundException)
            {
                return false;
            }
        }
    }
}