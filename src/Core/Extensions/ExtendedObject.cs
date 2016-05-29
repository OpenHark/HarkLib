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
        
        public static void To<TSource>(this TSource source, Action<TSource> action)
        {
            action(source);
        }
        public static TOut To<TSource, TOut>(this TSource source, Func<TSource, TOut> action)
        {
            return action(source);
        }
    }
}