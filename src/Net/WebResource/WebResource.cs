using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace HarkLib.Net
{
    public interface IWebResource
    {
        string Url
        {
            get;
        }
        
        string Name
        {
            get;
        }
        
        void ClearCache();
    }
}