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
            string pageContent = client.DownloadString("http://www.openbsd.org/");
            //string pageContent = "<a><b><c <def></def></b></a>";
            
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
                .ElementsByTagName("a")
                .Where(e => e.Attributes.ContainsKey("href"))
                .Count() != 40)
                return false;
            
            if(doc
                .ElementsByAttribute("rowspan", "2")
                .First()
                .Attributes["bgcolor"].Trim().ToLower() != "#007b9c")
                return false;
            
            if(doc.ElementsByTagName("td")
                .Count() != 5)
                return false;
            
            return true;
        }
    }
}