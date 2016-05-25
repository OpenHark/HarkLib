using System.Collections.Generic;

namespace System
{
    public static class ExtendedString
    {
        public static byte[] GetBytes(this string str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }
        
        public static int Count(this string str, string value)
        {
            int nb = 0;
            int index = -1;
            
            while((index = str.IndexOf(value, index + 1)) != -1)
                ++nb;
            
            return nb;
        }
        
        public static string[] Split(this string str)
        {
            return str.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new string[] { separator }, StringSplitOptions.None);
        }
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new string[] { separator }, options);
        }
        public static string[] Split(this string str, string separator, int nb)
        {
            return str.Split(new string[] { separator }, nb, StringSplitOptions.None);
        }
        public static string[] Split(this string str, string separator, int nb, StringSplitOptions options)
        {
            return str.Split(new string[] { separator }, nb, options);
        }
        
        public static List<string> SplitNotEscaped(
            this string str,
            char separator,
            char escapeChar = '\\',
            bool replaceEscaped = true,
            int nbMax = 0)
        {
            List<int> escapeFreeIndexes = new List<int>();
            List<string> strs = new List<string>();
            int lastIndex = 0;
            int index = 0;
            
            string doubleEscape = escapeChar.ToString() + escapeChar;
            string escapedStr = escapeChar.ToString() + separator;
            
            while((index = str.IndexOf(doubleEscape, index)) != -1)
            {
                index += 2;
                escapeFreeIndexes.Add(index);
            }
            
            index = 0;
            while((index = str.IndexOf(separator, index)) != -1)
            {
                if(nbMax > 0 && strs.Count >= nbMax - 1)
                    break;
                
                if(escapeFreeIndexes.Contains(index) || index == 0 || str[index - 1] != escapeChar)
                {
                    string newValue = str.Substring(lastIndex, index - lastIndex);
                    
                    if(replaceEscaped)
                        newValue = newValue.Replace(escapedStr, separator.ToString());
                    
                    strs.Add(newValue);
                    
                    lastIndex = index + 1;
                }
                
                ++index;
            }
            
            if(lastIndex != str.Length)
                strs.Add(str.Substring(lastIndex));
            
            return strs;
        }
    }   
}