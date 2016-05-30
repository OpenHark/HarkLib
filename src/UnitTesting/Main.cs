using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System;

using HarkLib.Core;

namespace UnitTesting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isVerbose = args.Length > 1 ? args[1].Trim() == "-v" : false;
            string nameFilter = args.Length > 0 ? args[0].Trim() : null;
            
            Console.WriteLine(" ::: Unit testing begins.");
            
            Regex rNameFilter = String.IsNullOrEmpty(nameFilter) ?
                new Regex("") : new Regex("^" + nameFilter);
            
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