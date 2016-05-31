using System;

using HarkLib.Parsers.Generic;

namespace UnitTesting.Parsers
{
    public class ParsersStreamSequencer : ITest
    {
        public override string Name
        {
            get
            {
                return "Parsers.Generic.StreamSequencer";
            }
        }
        
        public override bool Execute()
        {
            return new ParsersISequencer<StreamSequencer>(IsVerbose).Run();
        }
    }
}