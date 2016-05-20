using Hark.HarkPackageManager;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Numerics;
using System.Security;
using System.Linq;
using System.IO;
using System;

namespace Hark.HarkPackageManager.Library
{
    public static class SecureConsole
    {
        public static SecureString ReadPassword(char c = '*')
        {
            SecureString ss = new SecureString();
            
            while(true)
            {
                ConsoleKeyInfo ci = Console.ReadKey(true);
                
                switch(ci.Key)
                {
                    case ConsoleKey.Backspace:
                        if(ss.Length > 0)
                        {
                            ss.RemoveAt(ss.Length - 1);
                            Console.Write("\b \b");
                        }
                        break;
                        
                    case ConsoleKey.Enter:
                        Console.Write("\n");
                        return ss;
                        
                    default:
                        ss.AppendChar(ci.KeyChar);
                        Console.Write("*");
                        break;
                }
            }
        }
    }
}