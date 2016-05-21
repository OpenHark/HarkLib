namespace System
{
    public static class ExtendedInt64
    {
        public static byte[] GetBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}