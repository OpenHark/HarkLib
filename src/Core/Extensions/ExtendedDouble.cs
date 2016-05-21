namespace System
{
    public static class ExtendedDouble
    {
        public static byte[] GetBytes(this double value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}