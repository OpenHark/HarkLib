using System.Linq;
using System;

namespace Compiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Settings settings = new Settings(".make/config.ini");
            Cache.Instance = new Cache(settings);
            
            new Compilation[]
            {
                new CompilationCs(settings, "LibraryCore"),
                new CompilationFs(settings, "Parsers"),
                new CompilationCs(settings, "Parsers-Generic"),
                new CompilationCs(settings, "Security"),
                new CompilationCs(settings, "UnitTesting")
                
            }.ToList().ForEach(c => c.Execute());
            
            Cache.Instance.Save();
        }
    }
}