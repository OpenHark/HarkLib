using System;

using HarkLib.Parsers.Generic;

namespace UnitTesting.Parsers
{
    public class ParsersByteSequencer : ITest
    {
        public override string Name
        {
            get
            {
                return "Parsers.Generic.ByteSequencer";
            }
        }
        
        public override bool Execute()
        {
            return FunctionalTest() && ParsedTest() && DeepParsedTest() && Operators();
        }
        
        protected bool Operators()
        {
            var bs = new ByteSequencer("HTTP/1.1 404 Not found\r\nSuperHeader::master value\r\nSuperHeader!master value\r\nSuperHeader:-:master value\r\nHeader1: data1\r\nCookie: theme=light; sessionToken=abc123\r\nHeaderMultiple: x 1\r\nHeaderMultiple: x 2\r\nHeaderMultiple: x 3\r\nHeader2: data2\r\nHeader3: data3\r\nHeader4: data4\r\n\r\n      Hello! This is the body!");
            
            ParserResult root = bs
                | "HTTP/[version: ]"
                | "[i/code: ]"
                | "[message:\r\n]"
                | "[<headers:\r\n\r\n>]{[name:::!\r\n][|value|:\r\n|$]||[name:\\!!\r\n][|value|:\r\n|$]||[name::!\r\n][|value|:\r\n|$]}[</>]"
                | "[$s/body$]";
            
            if(IsVerbose)
            {
                Console.WriteLine(":: Operators");
            
                Console.WriteLine(" :!*********************: headers.<$name=HeaderMultiple$>.value");
                foreach(var e in root.GetAll<string>("headers.<$name=HeaderMultiple$>.value"))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
                
                Console.WriteLine("Version :");
                Console.WriteLine(root["version"]);
                Console.WriteLine("Code :");
                Console.WriteLine(root["code"]);
                Console.WriteLine("Message :");
                Console.WriteLine(root["message"]);
                
                Console.WriteLine("Headers :");
                
                Console.WriteLine("Cookies : " + root.GetString("headers.<|name=cookie|>.value") + "|");
                
                var cookies = ByteSequencer.Parse(
                    "[<cookies:$>][|name|:=][value:;|$][</>]",
                    root.GetString("headers.<|name=cookie|>.value")
                ).Close();
                
                foreach(var e in cookies.GetList("cookies"))
                    Console.WriteLine(e["name"] + " //=// " + e["value"]);
                    
                Console.WriteLine("Body :");
                Console.WriteLine(root["body"]);
            }
            
            return true;
        }
        
        protected bool FunctionalTest()
        {
            ParserResult root = new ByteSequencer("HTTP/1.1 404 Not found\r\nHeader1: data1\r\nErrorHeader\r\nHeader2: data2\r\nHeader3: data3\r\nHeader4: data4\r\nErrorHeaderFinal\r\n\r\nHello! This is the body!")
                .Until("version", " ")
                .Until("code", " ", converter : s => int.Parse(s))
                .Until("message", "\r\n")
                .RepeatUntil("headers", "\r\n\r\n", b => b
                    .Or(bb => bb
                        .Until("name", ":", validator : s => !s.Contains("\r\n"))
                        .Until("value", "\r\n", converter : s => s.Trim()),
                        bb => bb
                        .Until("error", "\r\n"),
                        bb => bb
                        .ToEnd("final", converter : bs => bs.GetString())))
                .ToEnd("body", converter : bs => bs.GetString())
                .Close();
            
            if(IsVerbose)
            {
                Console.WriteLine(":: FunctionalTest");
                
                Console.WriteLine("Version :");
                Console.WriteLine(root["version"]);
                Console.WriteLine("Code :");
                Console.WriteLine(root["code"]);
                Console.WriteLine("Message :");
                Console.WriteLine(root["message"]);
                
                Console.WriteLine("Headers :");
                foreach(var e in root.GetList("headers"))
                {
                    if(e.ContainsKey("final"))
                        Console.WriteLine(e["final"] + "|");
                    else if(e.ContainsKey("error"))
                        Console.WriteLine(e["error"] + "|");
                    else
                        Console.WriteLine(e["name"] + " = " + e["value"] + "|");
                }
                
                Console.WriteLine("Body :");
                Console.WriteLine(root["body"]);
            }
            
            if(!root["version"].Equals("HTTP/1.1"))
                return false;
                
            if(!root["code"].Equals(404))
                return false;
                
            if(!root["message"].Equals("Not found"))
                return false;
                
            if(!root["headers.<0>.name"].Equals("Header1"))
                return false;
            if(!root["headers.<0>.value"].Equals("data1"))
                return false;
                
            if(!root["headers.<1>.error"].Equals("ErrorHeader"))
                return false;
                
            if(!root["headers.<2>.name"].Equals("Header2"))
                return false;
            if(!root["headers.<2>.value"].Equals("data2"))
                return false;
                
            if(!root["headers.<3>.name"].Equals("Header3"))
                return false;
            if(!root["headers.<3>.value"].Equals("data3"))
                return false;
                
            if(!root["headers.<4>.name"].Equals("Header4"))
                return false;
            if(!root["headers.<4>.value"].Equals("data4"))
                return false;
                
            if(!root["headers.<5>.final"].Equals("ErrorHeaderFinal"))
                return false;
                
            if(!root["body"].Equals("Hello! This is the body!"))
                return false;
            
            return true;
        }
        
        protected bool DeepParsedTest()
        {
            ParserResult root = ByteSequencer.Parse(
                "[version: ][i/code: ][message:\r\n][<headers:\r\n\r\n>][name::][<values:$>][|vname|:=][|vvalue|:;|$][</>][</>][$s/body$]",
                "HTTP/1.1 404 Not found\r\nCookie: theme=light1; theme=light2; sessionToken=abc123\r\n\r\n\r\nHello! This is the body!"
            ).Close();
            
            if(IsVerbose)
            {
                string key = "headers.<$name=Cookie$>.values.<$vname=theme$>.vvalue";
                Console.WriteLine(" :!*********************: " + key);
                foreach(var e in root.GetAll<string>(key))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
                
                key = "headers.<$name=Cookie$>.values.<vname=theme>.vvalue";
                Console.WriteLine(" :!*********************: " + key);
                foreach(var e in root.GetAll<string>(key))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
                
                key = "headers.<name=Cookie>.values.<vname=theme>.vvalue";
                Console.WriteLine(" :!*********************: " + key);
                foreach(var e in root.GetAll<string>(key))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
                
                key = "headers.<name=Cookie>.values.<*>.vvalue";
                Console.WriteLine(" :!*********************: " + key);
                foreach(var e in root.GetAll<string>(key))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
            }
            
            return true;
        }
        
        protected bool ParsedTest()
        {
            ParserResult root = ByteSequencer.Parse(
                "[version: ][i/code: ][message:\r\n][<headers:\r\n\r\n>][name::][|value|:\r\n|$][</>][$|body|$]",
                "HTTP/1.1 404 Not found\r\nHeader1: data1\r\nCookie: theme=light; sessionToken=abc123\r\nHeaderMultiple: x 1\r\nHeaderMultiple: x 2\r\nHeaderMultiple: x 3\r\nHeader2: data2\r\nHeader3: data3\r\nHeader4: data4\r\n\r\n      Hello! This is the body!".GetBytes()
            ).Close();
            
            if(IsVerbose)
            {
                Console.WriteLine(":: ParsedTest");
            
                Console.WriteLine(" :!*********************: headers.<$name=HeaderMultiple$>.value");
                foreach(var e in root.GetAll<string>("headers.<$name=HeaderMultiple$>.value"))
                    Console.WriteLine(" :!: " + e);
                Console.WriteLine(" :!*********************: ");
                
                Console.WriteLine("Version :");
                Console.WriteLine(root["version"]);
                Console.WriteLine("Code :");
                Console.WriteLine(root["code"]);
                Console.WriteLine("Message :");
                Console.WriteLine(root["message"]);
                
                Console.WriteLine("Headers :");
                foreach(var e in root.GetList("headers"))
                    Console.WriteLine(e["name"] + " = " + e["value"] + "|");
                
                Console.WriteLine("Cookies :");
                
                var cookies = ByteSequencer.Parse(
                    "[<cookies:$>][|name|:=][value:;|$][</>]",
                    root.GetString("headers.<|name=cookie|>.value")
                ).Close();
                
                foreach(var e in cookies.GetList("cookies"))
                    Console.WriteLine(e["name"] + " //=// " + e["value"]);
                
                Console.WriteLine("Body :");
                Console.WriteLine(root["body"]);
            }
            
            if(!root["version"].Equals("HTTP/1.1"))
                return false;
                
            if(!root["code"].Equals(404))
                return false;
                
            if(!root["message"].Equals("Not found"))
                return false;
                
            if(!root["headers.<0>.name"].Equals("Header1"))
                return false;
            if(!root["headers.<0>.value"].Equals("data1"))
                return false;
                
            if(!root["headers.<5>.name"].Equals("Header2"))
                return false;
            if(!root["headers.<5>.value"].Equals("data2"))
                return false;
                
            if(!root["headers.<6>.name"].Equals("Header3"))
                return false;
            if(!root["headers.<6>.value"].Equals("data3"))
                return false;
                
            if(!root["headers.<7>.name"].Equals("Header4"))
                return false;
            if(!root["headers.<7>.value"].Equals("data4"))
                return false;
                
            if(!root["body"].Equals("Hello! This is the body!"))
                return false;
                
            return true;
        }
    }
}