#light

namespace HarkLib.Parsers

open System.Text.RegularExpressions
open System.Collections.Generic
open System

module public HTML =
    type internal StackValues =
    | TagOpen of string
    | AttributeName of string
    | AttributeValue of string
    | TagClose
    | DirectTagClose
    | CloseTag of string
    | Text of string
    | Comment of string
    
    let internal StackValuesToString = function
    | TagOpen(name) -> "TagOpen(" + name + ")"
    | AttributeName(name) -> "AttributeName(" + name + ")"
    | AttributeValue(value) -> "AttributeValue(" + value + ")"
    | TagClose -> "TagClose"
    | DirectTagClose -> "DirectTagClose"
    | CloseTag(name) -> "CloseTag(" + name + ")"
    | Text(value) -> "Text(" + value + ")"
    | Comment(value) -> "Comment(" + value + ")"
    
    type Comment(value : string) =
        class
            member this.Value = value
            
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
            
            abstract Elements : Element seq
            
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
    
    and Element(name : string) =
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
            
            member this.Name = name
            member this.Attributes = attributes
            
            override this.Elements = getElements this
            
            override this.ToString() =
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
                let ch =
                    this.Children
                    |> Seq.map (fun x -> x.ToString())
                    |> Seq.fold (+) ""
                "<" + name + at + ">" + ch + "</" + name + ">"
        end
        
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
            
            override this.ToString() =
                this.Children
                |> Seq.map (fun x -> x.ToString())
                |> Seq.fold (+) ""
        end
    
    type Fx =
        static member unstack stack =
            let h = List.head !stack
            stack := !stack |> List.tail
            h
        static member enstack stack value = stack := !stack @ [value]
        static member enstack2 stack value1 value2 = stack := !stack @ [value1 ; value2]
        static member enstack3 stack value1 value2 value3 = stack := !stack @ [value1 ; value2 ; value3]
        
        static member internal rex regex char = (new Regex(regex)).IsMatch(string(char))
        
        
        static member internal parseContentUntil (name : string) (content : string) stack = function
            | [] -> ()
            | '<'::'/'::e ->
                let rec isEndTag l r =
                    match (l, r) with
                    | ([], _)
                    | (_, []) -> false
                    | (x::[], x'::y::rn) when x = x' && Fx.rex "[ >]" y -> true
                    | (x::ln, x'::rn) when x = x' -> isEndTag ln rn
                    | _ -> false
                if isEndTag (name.ToCharArray() |> Array.toList) e then
                    Fx.enstack2 stack (Text content) (CloseTag name)
                    let rec unt = function
                    | [] -> ()
                    | ' '::e
                    | '>'::e -> Fx.parseRoot stack e
                    | _::e -> unt e
                    unt e
                else
                    Fx.parseContentUntil name (content + "</") stack e
                    
            | c::e ->
                Fx.parseContentUntil name (content + (string c)) stack e
        
        static member internal toRoot tagName stack e =
            match tagName with
            | "script" -> Fx.parseContentUntil tagName "" stack e
            | _ -> Fx.parseRoot stack e
        
        
        static member internal parseComment fnReturn (value : string) stack = function
            | [] -> ()
            | '-'::'-'::'>'::e ->
                Fx.enstack stack (StackValues.Comment value)
                fnReturn stack e
            | c::e ->
                Fx.parseComment fnReturn (value + (string c)) stack e
            
        static member internal parseAttributeValue (tagName : string) (value : string) (w : char) stack = function
            | [] -> ()
            | c::e when w <> char 0 && c = w ->
                Fx.enstack stack (AttributeValue value)
                Fx.parseAttributeStart tagName stack e
            | ' '::e when w = char 0 ->
                Fx.enstack stack (AttributeValue value)
                Fx.parseAttributeStart tagName stack e
            | '>'::e when w = char 0 ->
                Fx.enstack2 stack (AttributeValue value) TagClose
                Fx.toRoot tagName stack e
            | '/'::'>'::e when w = char 0 ->
                Fx.enstack3 stack (AttributeValue value) TagClose DirectTagClose
                Fx.toRoot tagName stack e
            | '\\'::c::e
            | c::e ->
                Fx.parseAttributeValue tagName (value + (string c)) w stack e
        
        static member internal parseAttributeValueStart2 (tagName : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.parseComment (Fx.parseAttributeValueStart2 tagName) "" stack e
            | '>'::e ->
                Fx.enstack stack TagClose
                Fx.toRoot tagName stack e
            | '/'::'>'::e ->
                Fx.enstack2 stack TagClose DirectTagClose
                Fx.toRoot tagName stack e
            | c::e when Fx.rex "[a-zA-Z0-9\\-_]" c ->
                Fx.parseAttributeValue tagName (string c) (char 0) stack e
            | ('"' & c)::e
            | ('\'' & c)::e ->
                Fx.parseAttributeValue tagName "" c stack e
            | _::e -> Fx.parseAttributeValueStart2 tagName stack e
            
        static member internal parseAttributeValueStart (tagName : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.parseComment (Fx.parseAttributeValueStart tagName) "" stack e
            | '>'::e ->
                Fx.enstack stack TagClose
                Fx.toRoot tagName stack e
            | '/'::'>'::e ->
                Fx.enstack2 stack TagClose DirectTagClose
                Fx.toRoot tagName stack e
            | '='::e -> Fx.parseAttributeValueStart2 tagName stack e
            | '\b'::e
            | ' '::e -> Fx.parseAttributeValueStart tagName stack e
            | c::e -> Fx.parseAttributeName tagName (string c) stack e
        
        static member internal parseAttributeName (tagName : string) (name : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.enstack stack (AttributeName name)
                Fx.parseComment (Fx.parseAttributeValueStart tagName) "" stack e
            | c::e when Fx.rex "[a-zA-Z0-9\\-_!\\[\\]]" c ->
                Fx.parseAttributeName tagName (name + (string c)) stack e
            | '>'::e ->
                Fx.enstack2 stack (AttributeName name) TagClose
                Fx.toRoot tagName stack e
            | '/'::'>'::e ->
                Fx.enstack3 stack (AttributeName name) TagClose DirectTagClose
                Fx.toRoot tagName stack e
            | '='::e ->
                Fx.enstack stack (AttributeName name)
                Fx.parseAttributeValueStart2 tagName stack e
            | _::e ->
                Fx.enstack stack (AttributeName name)
                Fx.parseAttributeValueStart tagName stack e
        
        static member internal parseAttributeStart (tagName : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.parseComment (Fx.parseAttributeStart tagName) "" stack e
            | ' '::e ->
                Fx.parseAttributeStart tagName stack e
            | '>'::e ->
                Fx.enstack stack TagClose
                Fx.toRoot tagName stack e
            | '/'::'>'::e ->
                Fx.enstack2 stack TagClose DirectTagClose
                Fx.toRoot tagName stack e
            | c::e ->
                Fx.parseAttributeName tagName (string c) stack e
        
        static member internal parseOpeningTagName (name : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.enstack stack (TagOpen name)
                Fx.parseComment (Fx.parseAttributeStart (name.Trim())) "" stack e
            | ' '::e ->
                Fx.enstack stack (TagOpen name)
                Fx.parseAttributeStart (name.Trim()) stack e
            | '>'::e ->
                Fx.enstack2 stack (TagOpen name) TagClose
                Fx.toRoot (name.Trim()) stack e
            | '/'::'>'::e ->
                Fx.enstack3 stack (TagOpen name) TagClose DirectTagClose
                Fx.toRoot (name.Trim()) stack e
            | c::e ->
                Fx.parseOpeningTagName (name + (string c)) stack e
        
        static member internal parseClosingTagName (name : string) stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.parseComment (Fx.parseClosingTagName name) "" stack e
            | '/'::'>'::e
            | '>'::e ->
                let fname = name.Trim()
                if fname.Length <> 0 then
                    Fx.enstack stack (CloseTag fname)
                Fx.parseRoot stack e
            | c::e ->
                Fx.parseClosingTagName (name + (string c)) stack e
        
        static member internal parseRootText (txt : string) stack = function
            | [] ->
                if txt.Length <> 0 then
                    Fx.enstack stack (Text txt)
            | '<'::'!'::'-'::'-'::e ->
                Fx.enstack stack (Text txt)
                Fx.parseComment (Fx.parseRootText "") "" stack e
            | '<'::'/'::e ->
                Fx.enstack stack (Text txt)
                Fx.parseClosingTagName "" stack e
            | '<'::e ->
                Fx.enstack stack (Text txt)
                Fx.parseOpeningTagName "" stack e
            | c::e ->
                Fx.parseRootText (txt + (string c)) stack e
                
        static member internal parseRoot stack = function
            | [] -> ()
            | '<'::'!'::'-'::'-'::e ->
                Fx.parseComment Fx.parseRoot "" stack e
            | '<'::'/'::e ->
                Fx.parseClosingTagName "" stack e
            | '<'::e ->
                Fx.parseOpeningTagName "" stack e
            | c::e ->
                Fx.parseRootText (string c) stack e
    
    let public Parse (value : string) =
        let stack = ref []
        
        value.ToCharArray()
        |> Array.toList
        |> Fx.parseRoot stack
        
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
            let elem = new Element(name)
            if isEmpty objStack then
                doc.Children.Add(elem)
            else
                (head objStack).Children.Add(elem)
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
            | x::e when x.Name.Trim().ToLower() = fname -> true
            | _::e -> look e
            
            if look !objStack then
                let rec close objStack =
                    if isEmpty objStack |> not then
                        if (Fx.unstack objStack :> Element).Name.Trim().ToLower() <> name then
                            close objStack
                close objStack
            else
                (head objStack).Children.Add(new Element(name))
            toObject objStack doc e
            
        | DirectTagClose::e ->
            Fx.unstack objStack |> ignore
            toObject objStack doc e
            
        | _::e ->
            toObject objStack doc e
        
        let doc = new Document()
        let objStack = ref []
        toObject objStack doc (!stack)
        doc
