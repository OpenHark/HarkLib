using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
            return FunctionalTest() && ParsedTest();
        }
        
        public bool FunctionalTest()
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
        
        protected bool ParsedTest()
        {
            ParserResult root = ByteSequencer.Parse(
                "[version: ][i/code: ][message:\r\n][<headers:\r\n\r\n>][name::][|value|:\r\n|$][</>][$s/body$]",
                "HTTP/1.1 404 Not found\r\nHeader1: data1\r\nCookie: theme=light; sessionToken=abc123\r\nHeader2: data2\r\nHeader3: data3\r\nHeader4: data4\r\n\r\nHello! This is the body!".GetBytes()
            ).Close();
			
            if(IsVerbose)
            {
                Console.WriteLine(":: ParsedTest");
                
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
                
            if(!root["body"].Equals("Hello! This is the body!"))
                return false;
            
            return true;
        }
    }
}