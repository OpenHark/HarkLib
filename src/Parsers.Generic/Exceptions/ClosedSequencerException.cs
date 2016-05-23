using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.Parsers.Generic
{
    [System.Serializable]
    public class ClosedSequencerException : System.Exception
    {
        public ClosedSequencerException()
        { }
        public ClosedSequencerException(string message)
            : base(message)
        { }
        public ClosedSequencerException(string message, System.Exception inner)
            : base(message, inner)
        { }
        protected ClosedSequencerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}