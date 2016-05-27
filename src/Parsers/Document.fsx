#light

namespace HarkLib.Parsers

open System.Text.RegularExpressions
open System.Collections.Generic
open System.Diagnostics
open System.Text
open System

/// <summary>
/// HTML comment.
/// </summary>
type Comment(value : string) =
    class
        member this.Value = value
        
        member this.ToString(sb : StringBuilder) =
            sb.Append("<!--").Append(value).Append("-->") |> ignore
            
        override this.ToString() = "<!--" + value + "-->"
    end
    
[<AbstractClass>]
type IElement() =
    class
        let children = new List<Object>()
        member this.Children = children
        
        member this.FlatChildren = seq {
            for c in this.Children do
                yield c
                match c with
                | :? IElement as e ->
                    yield! e.FlatChildren
                | _ -> ()
        }
        
        /// <summary>
        /// Provides the flat enumerator of all child elements.
        /// </summary>
        abstract Elements : Element seq
        
        /// <summary>
        /// Write the string form of this element in a StringBuilder.
        /// </summary>
        abstract ToString : StringBuilder -> unit
        
        override this.ToString() =
            let sb = new StringBuilder()
            this.ToString(sb)
            sb.ToString()
        
        member this.ElementsByTagName(tagName : string) = seq {
            let name = tagName.Trim().ToLower()
            for e in this.Elements do
                let elemName : string = e.Name
                if elemName.Trim().ToLower() = name then
                    yield e
        }
        
        member this.ElementsByAttribute(attrName : string, value : string) = seq {
            let name = attrName.Trim().ToLower()
            for e in this.Elements do
                let valid =
                    let attr : Dictionary<string, string> = e.Attributes
                    attr
                    |> Seq.exists (fun e -> e.Key.Trim().ToLower() = name && e.Value = value)
                if valid then
                    yield e
        }
    end

/// <summary>
/// HTML element representing a tag with its name,
/// its attributes and its children.
/// </summary>
and Element(name : string, parent : IElement, doc : Document) =
    class
        inherit IElement()
        let attributes = new Dictionary<string, string>()
        
        let rec getElements(el : Element) = seq {
            for c in el.Children do
                match c with
                | :? Element as e ->
                    yield e
                    yield! (getElements e)
                | _ -> ()
        }
        
        /// <summary>
        /// Tag name
        /// </summary>
        member this.Name =
            if name.Contains(":") then
                name.Substring(name.IndexOf(':') + 1)
            else
                name
        
        /// <summary>
        /// Tag namespace
        /// </summary>
        member this.Namespace =
            if name.Contains(":") then
                name.Substring(0, name.IndexOf(':'))
            else
                name
        
        /// <summary>
        /// Tag full name (name and namespace if specified)
        /// </summary>
        member this.FullName = name
        
        /// <summary>
        /// Tag attributes
        /// </summary>
        member this.Attributes = attributes
        
        /// <summary>
        /// Parent element
        /// </summary>
        member this.Parent = parent
        
        /// <summary>
        /// Document
        /// </summary>
        member this.Document = doc
        
        override this.Elements = getElements this
        
        override this.ToString(sb) =
            let matcher (x : Object) =
                match x with
                | :? string as s -> sb.Append(s) |> ignore
                | :? IElement as e -> e.ToString(sb)
                | :? Comment as c -> c.ToString(sb)
                | _ as x -> sb.Append(x.ToString()) |> ignore
            
            let at =
                attributes
                |> Seq.map (fun e -> (e.Key, e.Value))
                |> Seq.map (fun (a,b) ->
                    if b = null then
                        a
                    else
                        if b.Contains("\"") then
                            if b.Contains("'") then
                                a + "=\"" + (b.Replace("\"", "\\\"")) + "\""
                            else
                                a + "='" + b + "'"
                        else
                            a + "=\"" + b + "\""
                )
                |> Seq.fold (fun a b -> a + " " + b) ""
                
            sb.Append('<')
                .Append(name)
                .Append(at)
                .Append('>') |> ignore
            
            this.Children
            |> Seq.iter matcher
            
            sb.Append("</")
                .Append(name)
                .Append('>') |> ignore
        
            
    end
    
/// <summary>
/// HTML document representing the whole page.
/// </summary>
and Document() =
    class
        inherit IElement()
        
        override this.Elements = seq {
            for c in this.Children do
                match c with
                | :? Element as e ->
                    yield e
                    yield! e.Elements
                | _ -> ()
        }
        
        override this.ToString(sb) =
            let matcher (x : Object) =
                match x with
                | :? string as s -> sb.Append(s) |> ignore
                | :? IElement as e -> e.ToString(sb)
                | :? Comment as c -> c.ToString(sb)
                | _ as x -> sb.Append(x.ToString()) |> ignore
            
            this.Children
            |> Seq.iter matcher
    end
