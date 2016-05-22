using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace Compiler
{
    public abstract class Compilation
    {
        public Compilation(Settings settings, string prefix, string name)
        {
            this.settings = settings;
            this.prefix = prefix;
            
            this.Name = name;
        }
        
        private readonly Settings settings;
        private readonly string prefix;
        
        protected string Get(string key, string defaultValue = "")
        {
            return settings.Get(prefix + '-' + key, defaultValue);
        }
        protected List<string> GetList(string key)
        {
            return settings.GetList(prefix + '-' + key);
        }
        protected bool GetBool(string key, bool defaultValue = false)
        {
            return settings.GetBool(prefix + '-' + key, defaultValue);
        }
        
        protected string Name
        {
            get;
            private set;
        }
        
        protected string DestinationPath
        {
            get
            {
                return Get("DestinationPath");
            }
        }
        
        protected string CompilerName
        {
            get
            {
                return Get("CompilerName");
            }
        }
        
        protected string DocFullPath
        {
            get
            {
                return Get("DocFullPath");
            }
        }
        
        protected List<string> References
        {
            get
            {
                return GetList("References");
            }
        }
        
        protected string OutputName
        {
            get
            {
                return Get("OutputName");
            }
        }
        
        protected string SourcePath
        {
            get
            {
                return Get("SourcePath");
            }
        }
        
        protected string Target
        {
            get
            {
                return Get("Target");
            }
        }
        
        protected abstract void Compile();
        
        public void Execute()
        {
            if(!Directory.Exists(DestinationPath))
                Directory.CreateDirectory(DestinationPath);
            
            Compile();
            produced.Add(ProducedFormat(OutputFullPath));
        }
        
        protected string OutputFullPath
        {
            get
            {
                return Path.Combine(DestinationPath, OutputName);
            }
        }
        
        private static string ProducedFormat(string path)
        {
            string str = path.Replace("\\", "/");
            while(str.Contains("//"))
                str = str.Replace("//", "/");
            return str;
        }
        private static List<string> produced = new List<string>();
        
        protected bool HasBeenProduced(string outputFullPath)
        {
            return produced.Contains(ProducedFormat(outputFullPath));
        }
        
        protected IEnumerable<string> GetAllFiles(string path, string pattern)
        {
            foreach(string file in Directory.EnumerateFiles(path, pattern))
                yield return file;
            
            foreach(string dir in Directory.EnumerateDirectories(path))
                foreach(string file in GetAllFiles(dir, pattern))
                    yield return file;
        }
        protected IEnumerable<string> GetSurfaceFiles(string path, string pattern)
        {
            return Directory.EnumerateFiles(path, pattern);
        }
        
        protected string ReduceKey(string key, IEnumerable<string> list)
        {
            return list
                .Select(s => s.Trim())
                .Select(s => "\"" + key + s + "\" ")
                .Aggregate("", (a,b) => a + b);
        }
        
        protected string FindFromPath(string name)
        {
            return Environment.GetEnvironmentVariable("PATH")
                .Split(';')
                .Select(x => Path.Combine(x, name))
                .First(File.Exists);
        }
        
        protected string ReadStreamToEnd(StreamReader sr)
        {
            StringBuilder sb = new StringBuilder();
            
            try
            {
                while(sr.Peek() >= 0)
                    sb.Append(sr.ReadLine() + Environment.NewLine);
            }
            catch
            { }
            
            return sb.ToString();
        }
        
        protected void DisplayError(string msg, bool isException = false, string thrower = null)
        {
            string header = isException ? "===========[ EXCEPTION ]===========" : "=============[ ERROR ]=============";
            
            string h;
            if(String.IsNullOrEmpty(thrower) && GetBool("ShowErrorCommandLine", false))
                h = "";
            else
                h = " :: " + thrower + Environment.NewLine;
            
            Console.Error.WriteLine(header + Environment.NewLine + " :: " + Name + Environment.NewLine + h + msg + Environment.NewLine + "===================================");
        }
        
        protected void Die(bool result)
        {
            if(!result)
                Environment.Exit(0);
        }
        
        protected bool Run(string arguments, string filePath = null, string pattern = null)
        {
            filePath = filePath ?? CompilerName;
            
            Regex errorRegex = null;
            if(pattern != null)
                errorRegex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            string cmd;
            try
            {
                cmd = File.Exists(filePath) ? filePath : FindFromPath(filePath);
            }
            catch
            {
                DisplayError("Can't find the file \"" + filePath + "\" in the paths of the environment variable PATH.");
                return false;
            }
                
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(cmd, arguments);
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
                
                string output = ReadStreamToEnd(p.StandardOutput);
                string error = ReadStreamToEnd(p.StandardError);
                
                p.WaitForExit();
                
                bool regexMatched = errorRegex == null ? false : errorRegex.IsMatch(output);
                bool isGood = error.Length == 0 && !regexMatched;
                
                if(!isGood)
                {
                    if(regexMatched)
                        DisplayError(output, thrower : cmd);
                    else
                        DisplayError(error, thrower : cmd);
                }
                else
                    if(output.Length != 0)
                        Console.WriteLine(output);
                
                return isGood;
            }
            catch(Exception ex)
            {
                DisplayError(ex.Message, true, cmd);
                return false;
            }
        }
    }
}