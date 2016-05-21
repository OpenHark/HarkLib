namespace System
{
    public static class ExtendedBool
    {
        public static byte[] GetBytes(this bool value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}