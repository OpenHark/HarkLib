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
                    
                    bool multiple = currentName.StartsWith("$") && currentName.EndsWith("$");
                    if(multiple)
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
                        if(multiple)
                        {
                            ++index;
                            if(names.Length == index)
                            {
                                return list.Where(e => comparator(e[key] as string, value)).ToList();
                            }
                            else
                            {
                                return list.Where(e => comparator(e[key] as string, value))
                                    .Select(e => find(names, index, e))
                                    .ToList();
                            }
                        }
                        else
                        {
                            submap = list
                                .Where(e => comparator(e[key] as string, value))
                                .First();
                        }
                    }
                    catch
                    {
                        throw new NotFoundException();
                    }
                }
                else
                {
                    if(currentName == "*")
                    {
                        ++index;
                        if(names.Length == index)
                        {
                            return list.ToList();
                        }
                        else
                        {
                            return list
                                .Select(e => find(names, index, e))
                                .ToList();
                        }
                    }
                    else
                    {
                        int id = int.Parse(currentName);
                        submap = list[id];
                    }
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
        
        public Dictionary<string, List<T>> GetFlatMap<T>(string listPath, string keyName)
        {
            var list = GetList(listPath);
            
            Dictionary<string, List<T>> map = new Dictionary<string, List<T>>();
            
            Console.WriteLine("xxxxxxxxxxxxx " + listPath + " " + (list == null));
            foreach(Dico e in list)
            {
                map.Add(
                    key : e[keyName] as string,
                    value : e
                        .Where(v => v.Key != keyName)
                        .Select(v => v.Value)
                        .Cast<T>()
                        .ToList()
                );
            }
            
            return map;
        }
        public Dictionary<string, List<object>> GetFlatMap(string listPath, string keyName)
        {
            return GetFlatMap<object>(listPath, keyName);
        }
        
        public Dictionary<string, T> GetFlatFlatMap<T>(string listPath, string keyName = null)
        {
            var list = GetList(listPath);
            
            Dictionary<string, T> map = new Dictionary<string, T>();
            
            if(keyName == null)
            {
                foreach(Dico e in list)
                    map.Add(
                        key : e.Values.First() as string,
                        value : e.Values.Skip(1).Cast<T>().First()
                    );
            }
            else
            {
                foreach(Dico e in list)
                    map.Add(
                        key : e[keyName] as string,
                        value : e
                            .Where(v => v.Key != keyName)
                            .Select(v => v.Value)
                            .Cast<T>()
                            .First()
                    );
            }
            
            return map;
        }
        public Dictionary<string, object> GetFlatFlatMap(string listPath, string keyName)
        {
            return GetFlatFlatMap<object>(listPath, keyName);
        }
        
        public List<T> GetAll<T>(string path)
        {
            int nbC = Math.Min(path.Count("<$"), path.Count("$>"));
            int nbP = Math.Min(path.Count("<|$"), path.Count("$|>"));
            int nb = Math.Max(nbC, nbP) + path.Count("<*>");
            
            if(nb <= 0)
                return new List<T> { Get<T>(path) } ;
            
            IEnumerable<object> obj = Get<IEnumerable<object>>(path);
            for(int i = 0; i < nb - 1; ++i)
                obj = obj.Cast<IEnumerable<object>>().SelectMany(x => x);
            
            return obj.Cast<T>().ToList();
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