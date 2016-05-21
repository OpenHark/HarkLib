using System.Collections.Generic;

namespace System.Linq
{
    public static class ExtendedObject
    {
        public static IEnumerable<TSource> ToPipe<TSource>(this TSource source)
        {
            if(source == null)
                return new TSource[0];
            else
                return new TSource[] { source };
        }
    }
}