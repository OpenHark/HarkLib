using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System;

namespace UnitTesting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isVerbose = false;
            
            Console.WriteLine(" ::: Unit testing begins.");
            
            var tests = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(a => a.GetConstructor(Type.EmptyTypes) != null)
                .Select(Activator.CreateInstance)
                .OfType<ITest>();
            
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