using System.Diagnostics;
using System.Linq;
using System.Net;
using System;

namespace UnitTesting.Parsers
{
    public class Html : ITest
    {
        public override string Name
        {
            get
            {
                return "Parsers.Html.Parse";
            }
        }
        
        public override bool Execute()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2716.0 Safari/537.36 OPR/39.0.2234.0 (Edition developer)");
            string pageContent = client.DownloadString("https://www.google.fr/?gfe_rd=cr&ei=8F4_V5yPGK-s8weUw7-4Cw#q=Chocolate");
            //string pageContent = "<a html toto = \"12\" ><b><c id=\"answer-4606502\"></c></br></b></a>";
            
            HarkLib.Parsers.HTML.Document doc = null;
            
            if(IsVerbose)
            {
                Stopwatch sw = new Stopwatch();
                for(int i = 0; i < 4; ++i)
                {
                    sw.Reset();
                    sw.Start();
                    doc = HarkLib.Parsers.HTML.Parse(pageContent);
                    sw.Stop();
                    WriteLine(sw.Elapsed);
                }
            }
            else
                doc = HarkLib.Parsers.HTML.Parse(pageContent);
            
            if(doc
                .ElementsByTagName("div")
                .Where(e => e.Attributes.ContainsKey("id"))
                .Count() != 42)
                return false;
            
            if(doc
                .ElementsByAttribute("id", "main")
                .First()
                .Attributes["class"].Trim().ToLower() != "content")
                return false;
            
            if(doc.ElementsByTagName("script")
                .Select(e => " :: " + e.ToString())
                .Count() != 13)
                return false;
            
            return true;
        }
    }
}