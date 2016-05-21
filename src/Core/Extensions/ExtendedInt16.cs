namespace System
{
    public static class ExtendedInt16
    {
        public static byte[] GetBytes(this short value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}