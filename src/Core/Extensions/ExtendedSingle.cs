namespace System
{
    public static class ExtendedSingle
    {
        public static byte[] GetBytes(this float value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}