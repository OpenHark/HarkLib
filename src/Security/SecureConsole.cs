using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Security;
using System.Linq;
using System.IO;
using System;

namespace HarkLib.Security
{
    public static class SecureConsole
    {
        /// <summary>
        /// Read from the standard input a password and store it in a SecureString.
        /// </summary>
        /// <note>
        /// Better use <b>ReadSecurePassword</b> instead.
        /// </note>
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
        
        /// <summary>
        /// Read from the standard input a password and store it in a SecurePassword.
        /// </summary>
        public static SecurePassword ReadSecurePassword(char c = '*', int nbHashIterations = 500000)
        {
            return new SecurePassword(
                password : ReadPassword(c),
                nbHash : nbHashIterations,
                autoClear : true
            );
        }
    }
}