using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    [System.Serializable]
    public class UnrecognizedTypeException : System.Exception
    {
        public UnrecognizedTypeException()
        { }
        public UnrecognizedTypeException(string message)
            : base(message)
        { }
        public UnrecognizedTypeException(string message, System.Exception inner)
            : base(message, inner)
        { }
        protected UnrecognizedTypeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}