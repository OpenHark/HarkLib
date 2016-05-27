#light

namespace HarkLib.Parsers

open System.Text.RegularExpressions
open System.Collections.Generic
open System.Diagnostics
open System.Text
open System

/// <summary>
/// Provides a parser for HTML which doesn't throw exception and auto correct
/// the document.
/// </summary>
module public HTML =

    /// <summary>
    /// Parse a document and correct it if needed.
    /// </summary>
    /// <param name="document">String document to parse.</param>
    /// <returns>The parsed document.</returns>
    let public Parse (document : string) =
        let list = new List<string>()
        list.Add("script")
        XML.Parse document list
