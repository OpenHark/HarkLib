using System.Linq;
using System;

namespace Compiler
{
    public static class Program
    {
        public static Compilation GetFromName(Settings settings, string name)
        {
            string lname = name.ToLower();
            
            if(lname.StartsWith("cs"))
                return new CompilationCs(settings, name.Substring(3));
            else if(lname.StartsWith("fs"))
                return new CompilationFs(settings, name.Substring(3));
            else
                throw new Exception("Can't parse the project \"" + name + "\"!");
        }
        
        public static void Main(string[] args)
        {
            Settings settings = new Settings(".make/config.ini");
            Cache.Instance = new Cache(settings);
            
            settings.GetList("Projects")
                .Select(s => GetFromName(settings, s))
                .ToList()
                .ForEach(c => c.Execute());
            
            Cache.Instance.Save();
        }
    }
}