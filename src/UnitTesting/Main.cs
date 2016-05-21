using System.Diagnostics;
using System.Linq;
using System.Net;
using System;

namespace UnitTesting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Unit testing begin.");
            
            // TODO : Add unit tests calls here
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2716.0 Safari/537.36 OPR/39.0.2234.0 (Edition developer)");
            string downloadString = client.DownloadString("https://www.google.fr/?gfe_rd=cr&ei=8F4_V5yPGK-s8weUw7-4Cw#q=Chocolate");
            //string downloadString = "<a html toto = \"12\" ><b><c id=\"answer-4606502\"></c></br></b></a>";
            //var x = HarkLib.Parsers.HTML.Parse("<x></x><a abcd=\"tot\\\\\\\" o\"> <t/> bon<!-- comm -->jour <b></b></a>");
            Console.WriteLine(downloadString.Length);
            Console.WriteLine("****************************");
            HarkLib.Parsers.HTML.Document doc = null;
            Stopwatch sw = new Stopwatch();
            for(int i = 0; i < 4; ++i)
            {
                sw.Reset();
                sw.Start();
                doc = HarkLib.Parsers.HTML.Parse(downloadString);
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }
            Console.WriteLine("****************************");
            /*
            Console.WriteLine(doc
                .ElementsByTagName("div")
                .Where(e => e.Attributes.ContainsKey("id"))
                .Count());
              
            Console.WriteLine(doc
                .ElementsByAttribute("id", "main")
                .First()
                .Attributes["class"]);
                */
            
            //Console.WriteLine(doc);
            
                
            
            var y = doc.ElementsByTagName("script")
                .Select(e => " :: " + e.ToString())
                .Skip(2)
                .First();
            Console.WriteLine(y);/*
            Console.WriteLine("****************************");
            var z = doc.ElementsByTagName("script")
                .Select(e => " :: " + e.ToString())
                .Skip(0)
                .First();
            Console.WriteLine(z);
            Console.WriteLine("**************************** " + doc.ElementsByTagName("script").Count());
            *//*
            ((HarkLib.Parsers.HTML.Element)x.Children.First()).FlatChildren
                .ToList()
                .ForEach(s => Console.WriteLine(s));
                */
            //Console.WriteLine(((HarkLib.Parsers.HTML.Element)x.Children.First()).FlatChildren.GetType().Name);
            /*
            Console.WriteLine(x.ToString().Substring(0, 100));
            Console.WriteLine("Unit testing done.");*/
        }
    }
}