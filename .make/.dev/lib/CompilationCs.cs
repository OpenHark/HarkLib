using System.Collections.Generic;
using System.Linq;
using System;

namespace Compiler
{
    public class CompilationCs : Compilation
    {
        public CompilationCs(Settings settings, string name)
            : base(settings, "Cs-" + name, "C# " + name)
        { }
        
        protected List<string> Modules
        {
            get
            {
                return GetList("Modules");
            }
        }
        
        protected override void Compile()
        {
            IEnumerable<string> filesToCompile = SourcePath.Length > 0 ?
                GetAllFiles(SourcePath, "*.cs") : new string[0];
            
            if(filesToCompile.Any(Cache.Instance.NeedsUpdate) || Modules.Any(HasBeenProduced))
            {
                string files = filesToCompile
                    .Select(s => "\"" + s + "\"")
                    .Aggregate("", (a,b) => a + " " + b);
                
                string args = "/out:{DEST} /nowarn:3013 /target:{TARGET} {FILES} {MODULES} {DOC} /nologo {REFERENCES}"
                    .Replace("{DEST}", OutputFullPath)
                    .Replace("{TARGET}", Target)
                    .Replace("{FILES}", files)
                    .Replace("{MODULES}", ReduceKey("/addmodule:", Modules))
                    .Replace("{REFERENCES}", ReduceKey("/reference:", References))
                    .Replace("{DOC}", GetBool("GenerateDoc") ? "/doc:" + DocFullPath : "");
                
                Die(Run(args, pattern : "error [A-Z]{2}\\d{4}: (.*)$"));
                filesToCompile.ToList().ForEach(Cache.Instance.Encache);
                Console.WriteLine(Name + " compiled.");
            }
            else
                Console.WriteLine("Nothing to recompile for " + Name + ".");
        }
    }
}