namespace System
{
    public static class ExtendedByteArray
    {
        public static string GetString(this byte[] bytes)
        {
            return System.Text.Encoding.Default.GetString(bytes);
        }
        
        public static void Clear(this byte[] bytes)
        {
            Array.Clear(bytes, 0, bytes.Length);
        }
        
        public static bool ToBool(this byte[] bytes)
        {
            return BitConverter.ToBoolean(bytes, 0);
        }
        public static char ToChar(this byte[] bytes)
        {
            return BitConverter.ToChar(bytes, 0);
        }
        public static short ToInt16(this byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, 0);
        }
        public static int ToInt32(this byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }
        public static long ToInt64(this byte[] bytes)
        {
            return BitConverter.ToInt64(bytes, 0);
        }
        public static float ToSingle(this byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }
        public static float ToFloat(this byte[] bytes)
        {
            return bytes.ToSingle();
        }
        public static double ToDouble(this byte[] bytes)
        {
            return BitConverter.ToDouble(bytes, 0);
        }
    }   
}