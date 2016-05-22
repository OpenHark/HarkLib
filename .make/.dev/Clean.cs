using System.Linq;
using System.IO;
using System;

namespace Compiler
{
    public static class Program
    {
        private static bool Delete(string path)
        {
            try
            {
                if(File.Exists(path))
                {
                    Console.WriteLine(" ::File:: " + path);
                    File.Delete(path);
                }
                else if(Directory.Exists(path))
                {
                    Console.WriteLine(" ::Folder:: " + path);
                    Directory.Delete(path, true);
                }
                
                return true;
            }
            catch(IOException ex)
            {
                Console.Error.WriteLine(" /!\\ " + ex.Message);
                return false;
            }
        }
        
        public static void Main(string[] args)
        {
            Settings settings = new Settings(".make/config.ini");
            
            Console.WriteLine("Clearing...");
            
            bool result = settings
                .GetList("Clean")
                .Select(Delete)
                .Aggregate(true, (a,b) => a && b);
                
            if(result)
                Console.WriteLine("Cleaned.");
            else
                Console.WriteLine("An error occured.");
        }
    }
}