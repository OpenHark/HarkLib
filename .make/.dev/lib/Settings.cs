using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Compiler
{
    public class Settings
    {
        public Settings(string filePath)
        {
            this.Map = ReplaceEachOther(File.ReadAllText(filePath)
                .Replace("\r\n", "\n")
                .Replace("\\\n", "")
                .Split('\n')
                .Select(s => s.Trim())
                .Where(l => l.Length > 0)
                .Where(l => !l.StartsWith("#"))
                .Where(l => l.Contains("="))
                .Select(l => l.Contains("#") ? l.Substring(0, l.IndexOf("#")) : l)
                .Select(l => l.Split(new char[] { '=' }, 2))
                .Select(l => new string[]
                {
                    l[0].Trim(),
                    l[1].Trim()
                        .Replace("USER", Environment.UserName)
                        .Replace("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                })
                .ToList())
                .ToDictionary(ss => ss[0], ss => ss[1]);
        }
        
        private static string replaceThisOne(string e, List<string[]> m)
        {
            m.ForEach(s => e = e.Replace("$(" + s[0] + ")", s[1]));
            return e;
        }
        private static bool nm(List<string[]> ml)
        {
            bool changed = false;
            
            foreach(string[] ss in ml)
            {
                string temp = replaceThisOne(ss[1], ml);
                
                if(temp != ss[1])
                {
                    changed = true;
                    ss[1] = temp;
                }
            }
            
            return changed;
        }
        private static List<string[]> ReplaceEachOther(List<string[]> strs, int nb = 5)
        {
            for(int i = 0; i < nb; ++i)
            {
                if(!nm(strs) || nb == 0)
                    return strs;
            }
            
            return strs;
        }
        
        public Dictionary<string, string> Map
        {
            get;
            private set;
        }
        
        public string this[string key]
        {
            get
            {
                return Get(key, null);
            }
        }
        
        public bool GetBool(string key, bool defaultValue = false)
        {
            string result;
            
            if(Map.TryGetValue(key, out result))
                return new string[] { "y", "yes", "true", "on" }.Contains(result);
            else
                return defaultValue;
        }
        
        public string Get(string key, string defaultValue = null)
        {
            string result;
            
            if(Map.TryGetValue(key, out result))
                return result;
            else
                return defaultValue;
        }
        
        public List<string> GetList(string key, List<string> defaultValue = null)
        {
            defaultValue = defaultValue ?? new List<string>();
            string result;
            
            if(Map.TryGetValue(key, out result))
                return result.Split('|')
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();
            else
                return defaultValue;
        }
    }
}