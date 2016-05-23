using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    [System.Serializable]
    public class CommandNotRecognizedException : System.Exception
    {
        public CommandNotRecognizedException()
        { }
        public CommandNotRecognizedException(string message)
            : base(message)
        { }
        public CommandNotRecognizedException(string message, System.Exception inner)
            : base(message, inner)
        { }
        protected CommandNotRecognizedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}