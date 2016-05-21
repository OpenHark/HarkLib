namespace System
{
    public static class ExtendedString
    {
        public static byte[] GetBytes(this string str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }
    }   
}