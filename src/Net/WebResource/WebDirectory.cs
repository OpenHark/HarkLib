using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace HarkLib.Net
{
    public class WebDirectory : IWebResource
    {
        public WebDirectory(string url, string name = null)
        {
            this.Name = name;
            this.Url = url;
        }
        
        public string Url
        {
            get;
            private set;
        }
        
        public string Name
        {
            get;
            private set;
        }
        
        public string Host
        {
            get
            {
                return new Uri(Url).Host;
            }
        }
        
        private string _Page = null;
        private string Page
        {
            get
            {
                if(_Page == null)
                {
                    WebClient client = new WebClient();
                    _Page = client.DownloadString(Url);
                }
                return _Page;
            }
        }
        
        public void ClearCache()
        {
            _Directories = null;
            _Resources = null;
            _Files = null;
        }
        
        protected virtual IEnumerable<Tuple<string, string>> ParseRaw(IEnumerable<string> raw)
        {
            return raw
                .Select(s => s.Substring("href=\""))
                .Select(s => new Tuple<string, string>(Path.Combine(Url, s.SubstringUntil("\">")), s.Substring("\">", "</a>")));
        }
        
        private List<WebDirectory> _Directories = null;
        public List<WebDirectory> Directories
        {
            get
            {
                if(_Directories == null)
                {
                    _Directories = ParseRaw(Page.Split("alt=\"[DIR]\"")
                        .Skip(2)) // Skip starting and parent dir
                        .Select(s => new WebDirectory(s.Item1, s.Item2))
                        .ToList();
                }
                return _Directories;
            }
        }
        
        private List<WebFile> _Files = null;
        public List<WebFile> Files
        {
            get
            {
                if(_Files == null)
                {
                    _Files = ParseRaw(Page.Split("alt=\"[")
                        .Skip(2)
                        .Where(s => !s.StartsWith("DIR")))
                        .Select(s => new WebFile(s.Item1, s.Item2))
                        .ToList();
                }
                return _Files;
            }
        }
        
        private List<IWebResource> _Resources = null;
        public List<IWebResource> Resources
        {
            get
            {
                if(_Resources == null)
                {
                    _Resources = Directories
                        .Cast<IWebResource>()
                        .Concat(Files
                            .Cast<IWebResource>())
                        .ToList();
                }
                return _Resources;
            }
        }
    }
}