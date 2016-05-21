namespace System
{
    public static class ExtendedChar
    {
        public static byte[] GetBytes(this char value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}