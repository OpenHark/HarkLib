using System.Collections.Generic;
using System.Linq;
using System;

namespace Compiler
{
    public class CompilationFs : Compilation
    {
        public CompilationFs(Settings settings, string name)
            : base(settings, "Fs-" + name, "F# " + name)
        { }
        
        protected string EntryPointFileName
        {
            get
            {
                return Get("EntryPointFileName");
            }
        }
        
        protected string CommonFolder
        {
            get
            {
                return Get("Fs-CommonFolder");
            }
        }
        
        protected bool IsCommonTypeComplet
        {
            get
            {
                return Get("Fs-CommonFolderType", "complet").ToLower() == "complet";
            }
        }
        
        protected override void Compile()
        {
            IEnumerable<string> filesToCompile = SourcePath.Length > 0 ?
                GetAllFiles(SourcePath, "*.fsx") : new string[0];
                
            if(CommonFolder.Length > 0)
            {
                IEnumerable<string> common;
                if(IsCommonTypeComplet)
                    common = GetAllFiles(CommonFolder, "*.fsx");
                else
                    common = GetSurfaceFiles(CommonFolder, "*.fsx");
                filesToCompile = filesToCompile.Concat(common);
            }
            
            if(filesToCompile.Any(Cache.Instance.NeedsUpdate))
            {
                string files = filesToCompile
                    .Select(s => "\"" + s + "\"")
                    .Aggregate("", (a,b) => a + " " + b);
                
                string args = "--out:{DEST} --tailcalls+ --target:{TARGET} {FILES} {DOC} --nologo {REFERENCES}"
                    .Replace("{DEST}", OutputFullPath)
                    .Replace("{TARGET}", Target)
                    .Replace("{FILES}", files)
                    .Replace("{REFERENCES}", ReduceKey("--reference:", References))
                    .Replace("{DOC}", GetBool("GenerateDoc") ? "--doc:" + DocFullPath : "");
                
                Die(Run(args));
                filesToCompile.ToList().ForEach(Cache.Instance.Encache);
                Console.WriteLine(Name + " compiled.");
            }
            else
                Console.WriteLine("Nothing to recompile for " + Name + ".");
        }
    }
}