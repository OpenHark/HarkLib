using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace Compiler
{
    public class Cache
    {
        public Cache(Settings settings)
        {
            this.settings = settings;
            
            if(!File.Exists(CacheFile))
            {
                this.cache = new Dictionary<string, string>();
            }
            else
            {
                this.cache = File.ReadAllLines(CacheFile)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => l.Split(new char[] { ' ' }, 2))
                    .ToDictionary(ss => ss[1].Trim(), ss => ss[0].Trim());
            }
        }
        
        public static Cache Instance = null;
        
        private readonly Dictionary<string, string> cache;
        private readonly object mutex = new object();
        private readonly Settings settings;
        
        protected string FormatPath(string path)
        {
            path = path.Replace("\\", "/");
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
            
            lock(mutex)
                cache[filePath] = Hash(filePath);
        }
        
        public bool UseCache
        {
            get
            {
                return settings.GetBool("UseCache", false);
            }
        }
        
        public string CacheFile
        {
            get
            {
                return settings.Get("CacheFile", ".make-cache");
            }
        }
        
        public bool NeedsUpdate(string filePath)
        {
            filePath = FormatPath(filePath);
            
            lock(mutex)
            {
                return !UseCache
                    || !cache.ContainsKey(filePath)
                    || cache[filePath] != Hash(filePath);
            }
        }
        
        public void Save()
        {
            if(!UseCache)
                return;
            
            lock(mutex)
            {
                string content = cache
                    .Select(e => e.Value + " " + e.Key)
                    .Aggregate("", (a,b) => a + "\r\n" + b);
                File.WriteAllText(CacheFile, content);
            }
        }
    }
}