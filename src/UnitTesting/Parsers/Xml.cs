using System.Diagnostics;
using System.Linq;
using System.Net;
using System;

namespace UnitTesting.Parsers
{
    public class Xml : ITest
    {
        public override string Name
        {
            get
            {
                return "Parsers.Xml.Parse";
            }
        }
        
        public override bool Execute()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2716.0 Safari/537.36 OPR/39.0.2234.0 (Edition developer)");
            string pageContent = client.DownloadString("http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd");
            
            HarkLib.Parsers.Document doc = null;
            
            if(IsVerbose)
            {
                Stopwatch sw = new Stopwatch();
                for(int i = 0; i < 4; ++i)
                {
                    sw.Reset();
                    sw.Start();
                    doc = HarkLib.Parsers.XML.Parse(pageContent, null);
                    sw.Stop();
                    WriteLine(sw.Elapsed);
                }
            }
            else
                doc = HarkLib.Parsers.XML.Parse(pageContent, null);
            
            if(IsVerbose)
            {
                doc.ElementsByTagName("documentation")
                    .ForEach(e => Console.WriteLine(" :: " + e));
            }
            
            if(doc.ElementsByTagName("documentation").Count() != 5)
                return false;
            
            return true;
        }
    }
}