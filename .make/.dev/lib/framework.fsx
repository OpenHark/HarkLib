#light
#load "lib/configuration.fsx"

namespace MakeFramework

open System.Text.RegularExpressions
open System.Security.Cryptography
open System.Diagnostics
open System.Text
open System.IO
open System
        
module Settings =
    open Configuration.File
    do settings <- readSettings ".make/config.ini"
    
    let showErrorCommandLine = asBool <| getOr "ShowErrorCommandLine" "yes"
    let useCache = getOr "Cache" "no" |> asBool
    let cacheFile = ".make/.make-cache"
    let user = Environment.UserName
    let home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    
module Utils =
    let rec getAllFiles dir pattern = seq {
        yield! Directory.EnumerateFiles(dir, pattern)
        for d in Directory.EnumerateDirectories(dir) do
            yield! getAllFiles d pattern
    }
    let rec getSurfaceFiles dir pattern = seq {
        yield! Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly)
    }

    let toAsync (fn : unit -> bool) : Async<bool> = async {
        return fn()
    }
    
    exception CantFindFileInPath of string
    let findFileFromPath name =
        let p =
            Environment.GetEnvironmentVariable("PATH").Split(';')
            |> Seq.map (fun x -> Path.Combine(x, name))
            |> Seq.tryFind File.Exists
        match p with
        | None -> raise (CantFindFileInPath(name))
        | Some(x) -> x

module Cache =
    let hash (path : string) =
        use sha256 = new SHA256Managed()
        File.ReadAllBytes path
        |> sha256.ComputeHash
        |> BitConverter.ToString
        |> (fun s -> s.Replace("-", ""))
        
    let cacheMutex = new Object()
    
    let cache =
        if not (File.Exists(Settings.cacheFile)) then
            Map.empty
        else
            File.ReadAllLines Settings.cacheFile
            |> Seq.map (fun l -> l.Trim())
            |> Seq.filter (fun l -> l.Length > 0)
            |> Seq.map (fun l -> (l.Substring(0, l.IndexOf(" ")), l.Substring(l.IndexOf(" ") + 1)))
            |> Seq.map (fun (a,b) -> (b.Trim(), a.Trim()))
            |> Map.ofSeq
    let mutable newCache = cache
    
    let format (path : string) =
        let rec loop (p : string) =
            if p.Contains "\\\\" then
                p.Replace("\\\\", "\\") |> loop
            else
                p
        path.Replace("/", "\\") |> loop

    let mngCache path result = lock cacheMutex (fun () ->
        if Settings.useCache && result then
            let fpath = format path
            newCache <- newCache.Add(fpath, hash fpath)
        result
    )
        
    let needsUpdate (path : string) = lock cacheMutex (fun () ->
        let fpath = format path
        not Settings.useCache || (not(cache.ContainsKey(fpath)) || cache.[fpath] <> hash fpath)
    )
        
    let save () = lock cacheMutex (fun () ->
        if Settings.useCache then
            let txt =
                newCache
                |> Map.toSeq
                |> Seq.map (fun (k,v) -> v + " " + k)
                |> Seq.fold (fun s1 s2 -> s1 + "\r\n" + s2) ""
            File.WriteAllText(Settings.cacheFile, txt)
    )

module Compilation =
    let compile (compileFn : string -> bool) paths =
        let notify path = "Compiling \"" + path + "\"..." |> Console.WriteLine
        paths
        |> Seq.filter Cache.needsUpdate
        |> Seq.map (fun p -> (fun () -> notify p ; compileFn p))
        |> Seq.map Utils.toAsync
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Seq.forall (fun b -> b)
        
    let rec displayError name isException errorMsg (thrower : string) =
        let h =
            if thrower.Length > 0 && not Settings.showErrorCommandLine then
                ""
            else
                " :: " + thrower + Environment.NewLine
        let header = if isException then "===========[ EXCEPTION ]===========" else "=============[ ERROR ]============="
        
        header + Environment.NewLine + " :: " + name + Environment.NewLine + h + errorMsg + Environment.NewLine + "==================================="
        |> Console.Error.WriteLine
        
    let runGet name cmd arguments =
        let displayError = displayError name
        try
            let cmd = if File.Exists cmd then cmd else Utils.findFileFromPath cmd
            let procStartInfo = new System.Diagnostics.ProcessStartInfo(cmd, arguments)
            procStartInfo.RedirectStandardError <- true
            procStartInfo.RedirectStandardOutput <- true
            procStartInfo.UseShellExecute <- false
            procStartInfo.CreateNoWindow <- true
            let proc = new System.Diagnostics.Process()
            proc.StartInfo <- procStartInfo
            proc.Start() |> ignore
            
            let readStreamToEnd stream =
                try
                    let rec readLine (stream : StreamReader) (txt : string) =
                        match stream.ReadLine() with
                        | null -> txt
                        | line ->
                            readLine stream (txt + Environment.NewLine + line)
                    readLine stream ""
                with
                | _ -> ""
            
            let output = readStreamToEnd proc.StandardOutput
            let error = readStreamToEnd proc.StandardError
            proc.WaitForExit()
            (output, error)
        with
        | Utils.CantFindFileInPath(cmd) as ex ->
            displayError true
            <| "Can't find the file \"" + cmd + "\" in the paths of the environment variable PATH."
            <| cmd + " " + arguments
            raise ex
        | ex ->
            displayError true ex.Message <| cmd + " " + arguments
            raise ex
        
    let run name cmd arguments =
        let displayError = displayError name
        try
            let output, error = runGet name cmd arguments
            
            if String.length output <> 0 then
                output |> Console.WriteLine
            if String.length error <> 0 then
                displayError false error <| cmd + " " + arguments
            String.length error = 0
        with
        | _ -> false
            
    let runPattern name (spattern : string) cmd arguments =
        let pattern = new Regex(spattern, RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
        let displayError = displayError name
        try
            let output, error = runGet name cmd arguments
            
            let regexMatched = pattern.IsMatch(output)
            let isGood = String.length error = 0 && not regexMatched
            if not isGood then
                if regexMatched then
                    displayError false output <| cmd + " " + arguments
                else
                    displayError false error <| cmd + " " + arguments
            else
                if String.length output <> 0 then
                    output |> Console.WriteLine
            isGood
        with
        | _ -> false

