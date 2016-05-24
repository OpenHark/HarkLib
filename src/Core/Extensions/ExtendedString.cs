namespace System
{
    public static class ExtendedString
    {
        public static byte[] GetBytes(this string str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }
        
        public static int Count(this string str, string value)
        {
            int nb = 0;
            int index = -1;
            
            while((index = str.IndexOf(value, index + 1)) != -1)
                ++nb;
            
            return nb;
        }
    }   
}