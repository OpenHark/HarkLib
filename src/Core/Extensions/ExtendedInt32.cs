namespace System
{
    public static class ExtendedInt32
    {
        public static byte[] GetBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }
        
        public static string ToPlural(this int value, string pluralString = "s", string singularString = "")
        {
            return value >= 2 ? pluralString : singularString;
        }
    }
}