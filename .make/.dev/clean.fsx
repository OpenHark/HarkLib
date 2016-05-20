#light

#load "lib/framework.fsx"

open System.Security.Cryptography
open System.Diagnostics
open System.Text
open System.IO
open System

open MakeFramework
open Configuration.File

let deletePath (path : string) =
    try
        if File.Exists path then
            " ::File:: " + path |> Console.WriteLine
            File.Delete path
        else if Directory.Exists path then
            " ::Folder:: " + path |> Console.WriteLine
            Directory.Delete(path, true)
        true
    with
    | :? IOException as ex ->
        " /!\\ " + ex.Message |> Console.Error.WriteLine
        false

let filesToClean = getOr "Clean" "" |> asArray

Console.WriteLine "Clearing..."

let result =
    filesToClean
    |> Seq.map deletePath
    |> Seq.fold (&&) true

if result then
    Console.WriteLine "Cleaned."
else
    Console.WriteLine "An error occured."
