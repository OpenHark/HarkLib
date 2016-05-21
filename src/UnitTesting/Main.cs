using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Net;
using System;

namespace UnitTesting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isVerbose = false;
            string nameFilter = "Parsers\\.Html";
            
            Console.WriteLine(" ::: Unit testing begins.");
            
            Regex rNameFilter = nameFilter == null ? new Regex("") : new Regex("^" + nameFilter);
            
            var tests = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(a => a.GetConstructor(Type.EmptyTypes) != null)
                .Select(Activator.CreateInstance)
                .OfType<ITest>()
                .Where(t => rNameFilter.IsMatch(t.Name));
            
            foreach(ITest test in tests)
            {
                try
                {
                    test.IsVerbose = isVerbose;
                    
                    if(isVerbose)
                        Console.WriteLine(" (i) Testing " + test.Name + "...");
                    
                    if(test.Execute())
                        Console.WriteLine(" [o] " + test.Name + " passed.");
                    else
                        Console.Error.WriteLine(" [x] Error in " + test.Name + ".");
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(" [x] Exception in " + test.Name + " : " + ex.Message);
                }
            }
            
            Console.WriteLine(" ::: Unit testing done.");
        }
    }
}