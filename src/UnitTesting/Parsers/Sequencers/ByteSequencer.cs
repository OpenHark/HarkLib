using System;

using HarkLib.Parsers.Generic;

namespace UnitTesting.Parsers
{
    public class ParsersByteSequencer : ITest
    {
        public override string Name
        {
            get
            {
                return "Parsers.Generic.ByteSequencer";
            }
        }
        
        public override bool Execute()
        {
            return new ParsersISequencer<ByteSequencer>(IsVerbose).Run();
        }
    }
}