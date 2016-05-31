using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Core
{
    public class FileCache
    {
        public FileCache(string cacheFilePath)
        {
            this.CacheFilePath = cacheFilePath;
            this.Enable = true;
            
            if(!File.Exists(CacheFilePath))
            {
                this.cache = new Dictionary<string, string>();
            }
            else
            {
                this.cache = File.ReadAllLines(CacheFilePath)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => l.Split(new char[] { ' ' }, 2))
                    .ToDictionary(ss => ss[1].Trim(), ss => ss[0].Trim());
            }
        }
        
        private readonly Dictionary<string, string> cache;
        private readonly object mutex = new object();
        
        public bool Enable
        {
            get;
            set;
        }
        
        public string CacheFilePath
        {
            get;
            private set;
        }
        
        protected string FormatPath(string path)
        {
            path = path.Trim().Replace("\\", "/");
            while(path.Contains("//"))
                path = path.Replace("//", "/");
            return path;
        }
        
        protected string Hash(string filePath)
        {
            using(var sha = new SHA256Managed())
            {
                var fbs = File.ReadAllBytes(filePath);
                var bs = sha.ComputeHash(fbs);
                var str = BitConverter.ToString(bs);
                return str.Replace("-", "");
            }
        }
        
        public void Encache(string filePath)
        {
            filePath = FormatPath(filePath);
            
            string hash = Hash(filePath);
            lock(mutex)
                cache[filePath] = hash;
        }
        
        public bool Changed(string filePath)
        {
            filePath = FormatPath(filePath);
            
            lock(mutex)
            {
                return !Enable
                    || !cache.ContainsKey(filePath)
                    || cache[filePath] != Hash(filePath);
            }
        }
        
        public void Save(string cacheFilePath = null)
        {
            cacheFilePath = cacheFilePath ?? CacheFilePath;
            
            string content;
            lock(mutex)
            {
                content = cache
                    .Select(e => e.Value + " " + e.Key)
                    .Aggregate("", (a,b) => a + "\r\n" + b);
            }
            
            File.WriteAllText(cacheFilePath, content);
        }
    }
}