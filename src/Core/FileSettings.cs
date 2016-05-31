using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Core
{
    public class FileSettings
    {
        public FileSettings(
            string filePath,
            string[][] defaultVariables = null,
            int nbReplacementIterations = 5)
        {
            this.YesValues = DefaultYesValues;
            
            defaultVariables = defaultVariables ?? new string[0][];
            
            if(!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            
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
                })
                .Concat(new string[][]
                {
                    new string[] { "USER", Environment.UserName },
                    new string[] { "HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) }
                })
                .Concat(defaultVariables)
                .ToList(), nbReplacementIterations)
                .ToDictionary(ss => ss[0], ss => ss[1]);
        }
        
        public static readonly string[] DefaultYesValues = new string[]
        {
            "y",
            "yes",
            "true",
            "on"
        };
        
        public string this[string key]
        {
            get
            {
                return Get(key, null);
            }
        }
        
        public Dictionary<string, string> Map
        {
            get;
            private set;
        }
        
        public string[] YesValues
        {
            get;
            set;
        }
        
        private static bool IterateTransform(List<string[]> ml)
        {
            bool changed = false;
            
            foreach(string[] ss in ml)
            {
                string temp = ss[1];
                foreach(string[] s in ml)
                    temp = temp.Replace("$(" + s[0] + ")", s[1]);
                
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
                if(!IterateTransform(strs))
                    break;
            }
            
            return strs;
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            string result;
            
            if(Map.TryGetValue(key, out result))
                return int.Parse(result);
            else
                return defaultValue;
        }
        
        public bool GetBool(string key, bool defaultValue = false)
        {
            string result;
            
            if(Map.TryGetValue(key, out result))
                return YesValues.Contains(result);
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
        
        public string Compute(string value, int nbReplacementIterations = 5)
        {
            for(int i = 0; i < nbReplacementIterations; ++i)
                foreach(var s in Map)
                    value = value.Replace("$(" + s.Key + ")", s.Value);
                    
            return value;
        }
    }
}