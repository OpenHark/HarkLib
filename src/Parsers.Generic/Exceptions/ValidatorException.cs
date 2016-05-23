using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    [System.Serializable]
    public class ValidatorException : System.Exception
    {
        public ValidatorException()
        { }
        public ValidatorException(string message)
            : base(message)
        { }
        public ValidatorException(string message, System.Exception inner)
            : base(message, inner)
        { }
        protected ValidatorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}