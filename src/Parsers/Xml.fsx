#light

namespace HarkLib.Parsers

open System.Text.RegularExpressions
open System.Collections.Generic
open System.Diagnostics
open System.Text
open System

/// <summary>
/// Provides a parser for XML which doesn't throw exception and auto correct
/// the document.
/// </summary>
module public XML =
    type private StackValues =
    | TagOpen of string
    | AttributeName of string
    | AttributeValue of string
    | TagClose
    | DirectTagClose
    | CloseTag of string
    | Text of string
    | Comment of string
    
    let private StackValuesToString = function
    | TagOpen(name) -> "TagOpen(" + name + ")"
    | AttributeName(name) -> "AttributeName(" + name + ")"
    | AttributeValue(value) -> "AttributeValue(" + value + ")"
    | TagClose -> "TagClose"
    | DirectTagClose -> "DirectTagClose"
    | CloseTag(name) -> "CloseTag(" + name + ")"
    | Text(value) -> "Text(" + value + ")"
    | Comment(value) -> "Comment(" + value + ")"
    
    type private ParserState =
    | EndParsing
    | ParseContentUntil of char list * StringBuilder // name * content
    | ToRoot
    | ParseComment of ParserState * StringBuilder // fnReturn * value
    | ParseAttributeValue of StringBuilder * char // value * w
    | ParseAttributeValueStart2
    | ParseAttributeValueStart
    | ParseAttributeName of StringBuilder // name
    | ParseAttributeStart
    | ParseOpeningTagName of StringBuilder // name
    | ParseClosingTagName of StringBuilder // name
    | ParseRootText of StringBuilder // txt
    | ParseRoot
    
    /// <summary>
    /// Parse a document and correct it if needed.
    /// </summary>
    /// <param name="document">String document to parse.</param>
    /// <returns>The parsed document.</returns>
    let public Parse (document : string) (codeTags0 : List<string>) =
        let codeTags = if codeTags0 = null then new List<string>() else codeTags0
        
        let stack = ref []
        
        let rrAttName = new Regex("[a-zA-Z0-9\\-_!\\[\\]:]")
        let rexAttName char = rrAttName.IsMatch(string char)
        
        let rrAttValue = new Regex("[a-zA-Z0-9\\-_]")
        let rexAttValue char = rrAttValue.IsMatch(string char)
        
        let rrSpace = new Regex("[ \b\r\n]")
        let rexSpace char = rrSpace.IsMatch(string char)
        
        let unstack stack =
            let h : Element = List.head !stack
            stack := !stack |> List.tail
            h
        
        let rec parseAll (tagName : string) stack data state =
            let enstack stack value = stack := !stack @ [value]
            let enstack2 stack value1 value2 = stack := !stack @ [value1 ; value2]
            let enstack3 stack value1 value2 value3 = stack := !stack @ [value1 ; value2 ; value3]
            
            let newTagName, newData, newState =
                match state with
                | ParseContentUntil(name, content) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'/'::e ->
                        let rec isEndTag l r =
                            match (l, r) with
                            | ([], _)
                            | (_, []) -> false
                            | (x::[], x'::y::rn) when x = x' && (rexSpace y || y = '>') -> true
                            | (x::ln, x'::rn) when x = x' -> isEndTag ln rn
                            | _ -> false
                        if isEndTag name e then
                            enstack2 stack (Text (content.ToString())) (CloseTag (new string (name |> List.toArray)))
                            let rec unt = function
                            | [] -> []
                            | '>'::e -> e
                            | _::e -> unt e
                            (null, (unt e), ParseRoot)
                        else
                            (null, e, ParseContentUntil(name, content.Append("</")))
                            
                    | c::e ->
                        (null, e, ParseContentUntil(name, content.Append(c)))
                
                | ToRoot ->
                    if codeTags.Contains(tagName.ToLower()) then
                        (tagName, data, ParseContentUntil(tagName.ToCharArray() |> Array.toList, new StringBuilder()))
                    else
                        (tagName, data, ParseRoot)
                
                | ParseComment(fnReturn, value) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '-'::'-'::'>'::e ->
                        enstack stack (StackValues.Comment(value.ToString()))
                        (null, e, fnReturn)
                    | c::e ->
                        (null, e, ParseComment(fnReturn, value.Append(c)))
                    
                | ParseAttributeValue(value, w) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | c::e when w <> char 0 && c = w ->
                        enstack stack (AttributeValue (value.ToString()))
                        (null, e, ParseAttributeStart)
                    | x::e when w = char 0 && rexSpace x ->
                        enstack stack (AttributeValue (value.ToString()))
                        (null, e, ParseAttributeStart)
                    | '>'::e when w = char 0 ->
                        enstack2 stack (AttributeValue (value.ToString())) TagClose
                        (null, e, ToRoot)
                    | '/'::'>'::e when w = char 0 ->
                        enstack3 stack (AttributeValue (value.ToString())) TagClose DirectTagClose
                        (null, e, ToRoot)
                    | '\\'::c::e
                    | c::e ->
                        (null, e, ParseAttributeValue(value.Append(c), w))
                
                | ParseAttributeValueStart2 ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        (null, e, ParseComment(ParseAttributeValueStart2, new StringBuilder()))
                    | '>'::e ->
                        enstack stack TagClose
                        (null, e, ToRoot)
                    | '/'::'>'::e ->
                        enstack2 stack TagClose DirectTagClose
                        (null, e, ToRoot)
                    | ('"' & c)::e
                    | ('\'' & c)::e ->
                        (null, e, ParseAttributeValue(new StringBuilder(), c))
                    | c::e when rexAttValue c ->
                        (null, e, ParseAttributeValue(new StringBuilder(string c), char 0))
                    | _::e ->
                        (null, e, ParseAttributeValueStart2)
                    
                | ParseAttributeValueStart ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        (null, e, ParseComment(ParseAttributeValueStart, new StringBuilder()))
                    | '>'::e ->
                        enstack stack TagClose
                        (null, e, ToRoot)
                    | '/'::'>'::e ->
                        enstack2 stack TagClose DirectTagClose
                        (null, e, ToRoot)
                    | '='::e ->
                        (null, e, ParseAttributeValueStart2)
                    | '\b'::e
                    | ' '::e ->
                        (null, e, ParseAttributeValueStart)
                    | c::e ->
                        (null, e, ParseAttributeName(new StringBuilder(string c)))
                
                | ParseAttributeName(name) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        enstack stack (AttributeName (name.ToString()))
                        (null, e, ParseComment(ParseAttributeValueStart, new StringBuilder()))
                    | c::e when rexAttName c ->
                        (null, e, ParseAttributeName(name.Append(c)))
                    | '>'::e ->
                        enstack2 stack (AttributeName (name.ToString())) TagClose
                        (null, e, ToRoot)
                    | '/'::'>'::e ->
                        enstack3 stack (AttributeName (name.ToString())) TagClose DirectTagClose
                        (null, e, ToRoot)
                    | '='::e ->
                        enstack stack (AttributeName (name.ToString()))
                        (null, e, ParseAttributeValueStart2)
                    | _::e ->
                        enstack stack (AttributeName (name.ToString()))
                        (null, e, ParseAttributeValueStart)
                
                | ParseAttributeStart ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        (null, e, ParseComment(ParseAttributeStart, new StringBuilder()))
                    | x::e when rexSpace x ->
                        (null, e, ParseAttributeStart)
                    | '>'::e ->
                        enstack stack TagClose
                        (null, e, ToRoot)
                    | '<'::e ->
                        enstack stack TagClose
                        ("", e, ParseOpeningTagName (new StringBuilder()))
                    | '/'::'>'::e ->
                        enstack2 stack TagClose DirectTagClose
                        (null, e, ToRoot)
                    | c::e ->
                        (null, e, ParseAttributeName(new StringBuilder(string c)))
                
                | ParseOpeningTagName(name) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        let tname = name.ToString().Trim()
                        enstack stack (TagOpen tname)
                        (tname, e, ParseComment(ParseAttributeStart, new StringBuilder()))
                    | x::e when rexSpace x ->
                        let tname = name.ToString().Trim()
                        enstack stack (TagOpen tname)
                        (tname, e, ParseAttributeStart)
                    | '>'::e ->
                        let tname = name.ToString().Trim()
                        enstack2 stack (TagOpen tname) TagClose
                        (tname, e, ToRoot)
                    | '<'::e ->
                        let tname = name.ToString().Trim()
                        enstack2 stack (TagOpen tname) TagClose
                        ("", e, ParseOpeningTagName (new StringBuilder()))
                    | '/'::'>'::e ->
                        let tname = name.ToString().Trim()
                        enstack3 stack (TagOpen tname) TagClose DirectTagClose
                        (tname, e, ToRoot)
                    | c::e ->
                        let tname = name.Append(c)
                        (null, e, ParseOpeningTagName(tname))
                
                | ParseClosingTagName(name) ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        (null, e, ParseComment(ParseClosingTagName name, new StringBuilder()))
                    | '/'::'>'::e
                    | '>'::e ->
                        let fname = name.ToString().Trim()
                        if fname.Length <> 0 then
                            enstack stack (CloseTag fname)
                        ("", e, ParseRoot)
                    | c::e ->
                        (null, e, ParseClosingTagName(name.Append(c)))
                
                | ParseRootText(txt) ->
                    match data with
                    | [] ->
                        if txt.Length <> 0 then
                            enstack stack (Text (txt.ToString()))
                        (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        enstack stack (Text (txt.ToString()))
                        (null, e, ParseComment(ParseRootText (new StringBuilder()), (new StringBuilder())))
                    | '<'::'/'::e ->
                        enstack stack (Text (txt.ToString()))
                        ("", e, ParseClosingTagName (new StringBuilder()))
                    | '<'::e ->
                        enstack stack (Text (txt.ToString()))
                        ("", e, ParseOpeningTagName (new StringBuilder()))
                    | c::e ->
                        ("", e, ParseRootText (txt.Append(c)))
                        
                | _ ->
                    match data with
                    | [] -> (null, [], EndParsing)
                    | '<'::'!'::'-'::'-'::e ->
                        (null, e, ParseComment(ParseRoot, (new StringBuilder())))
                    | '<'::'/'::e ->
                        ("", e, ParseClosingTagName (new StringBuilder()))
                    | '<'::e ->
                        ("", e, ParseOpeningTagName (new StringBuilder()))
                    | c::e ->
                        ("", e, ParseRootText (new StringBuilder(string c)))
                        
            match newState with
            | EndParsing -> ()
            | _ -> parseAll (if newTagName = null then tagName else newTagName) stack newData newState
        
        parseAll "" stack (document.ToCharArray() |> Array.toList) ParseRoot
        
        let enstack stack value = stack := [value] @ !stack
        let head objref = List.head !objref
        let isEmpty objref = List.isEmpty !objref
        let rec toObject (objStack : Element list ref) (doc : Document) = function
        | [] -> ()
        
        | StackValues.Comment(txt)::e ->
            let comment = new Comment(txt)
            if isEmpty objStack then
                doc.Children.Add(comment)
            else
                (head objStack).Children.Add(comment)
            toObject objStack doc e
        
        | Text(txt)::e ->
            if isEmpty objStack then
                doc.Children.Add(txt)
            else
                (head objStack).Children.Add(txt)
            toObject objStack doc e
            
        | TagOpen(name)::e ->
            let topElement = if isEmpty objStack then doc :> IElement else head objStack :> IElement
            let elem = new Element(name, topElement, doc)
            topElement.Children.Add(elem)
            enstack objStack elem
            toObject objStack doc e
            
        | AttributeName(attName)::AttributeValue(attValue)::e ->
            let atts = (head objStack).Attributes
            if atts.ContainsKey(attName) |> not then
                atts.Add(attName, attValue)
            toObject objStack doc e
            
        | AttributeName(attName)::e ->
            let atts = (head objStack).Attributes
            if atts.ContainsKey(attName) |> not then
                atts.Add(attName, null)
            toObject objStack doc e
            
        | CloseTag(name)::e ->
            let fname = name.Trim().ToLower()
            let rec look : Element list -> bool = function
            | [] -> false
            | x::e when x.FullName.Trim().ToLower() = fname -> true
            | _::e -> look e
            
            if look !objStack then
                let rec close objStack =
                    if isEmpty objStack |> not then
                        if (unstack objStack).FullName.Trim().ToLower() <> fname then
                            close objStack
                close objStack
            else
                let topElement = head objStack
                topElement.Children.Add(new Element(name, topElement, doc))
            toObject objStack doc e
            
        | DirectTagClose::e ->
            unstack objStack |> ignore
            toObject objStack doc e
            
        | _::e ->
            toObject objStack doc e
        
        let doc = new Document()
        let objStack = ref []
        toObject objStack doc (!stack)
        doc
