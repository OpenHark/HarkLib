using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net;
using System;

namespace HarkLib.Net
{
    public class WebServerMethod : Attribute
    {
        public WebServerMethod(string pattern)
        {
            this.Pattern = pattern;
            this.Method = "get";
        }
        
        public string Pattern
        {
            get;
            private set;
        }
        
        private string _Method;
        public string Method
        {
            get
            {
                return _Method;
            }
            set
            {
                _Method = value.Trim().ToLower();
            }
        }
    }
}