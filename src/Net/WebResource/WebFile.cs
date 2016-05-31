using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace HarkLib.Net
{
    public class WebFile : IWebResource
    {
        public WebFile(string url, string name = null)
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
        
        private byte[] _Content = null;
        public byte[] Content
        {
            get
            {
                if(_Content == null)
                {
                    WebClient client = new WebClient();
                    _Content = client.DownloadData(Url);
                }
                return _Content;
            }
        }
        
        public void ClearCache()
        {
            _Content = null;
        }
    }
}