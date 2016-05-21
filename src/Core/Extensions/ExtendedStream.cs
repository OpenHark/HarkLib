using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class ExtendedStream
    {
        public static char ReadChar(this Stream stream)
        {
            return (char)stream.ReadByte();
        }
        
        public static string ReadWord(this Stream stream, char[] endChars = null)
        {
            bool wrapped = false;
            StringBuilder sb = new StringBuilder();
            
            endChars = endChars ?? new char[] { '\t', '\r', '\n', ' ' };
            
            try
            {
                char c;
                do
                {
                    c = stream.ReadChar();
                    if(wrapped)
                    {
                        if(c == '\"')
                        {
                            wrapped = sb.Length == 0 || sb[sb.Length - 1] != '\\';
                            if(!wrapped)
                                continue;
                        }
                    }
                    sb.Append(c);
                } while(wrapped || endChars.All(cx => cx != c));
            }
            catch(System.IO.IOException)
            { }
            
            return sb.ToString();
        }
        
        public static byte[] ReadUntilTimeout(this Stream stream, int timeout = -1, bool firstByteBlocking = false)
        {
            if(timeout > -1)
                stream.ReadTimeout = timeout;
            
            using(MemoryStream ms = new MemoryStream())
            {
                if(firstByteBlocking)
                    ms.WriteByte((byte)stream.ReadByte());
                
                try
                {
                    stream.CopyTo(ms);
                }
                catch
                { }
                
                return ms.ToArray();
            }
        }
        
        public static byte[] ReadUntil(this Stream stream, byte limitValue, bool included = false)
        {
            List<byte> data = new List<byte>();
            
            int value;
            while((value = stream.ReadByte()) != limitValue)
                data.Add((byte)value);
            
            if(included)
                data.Add(limitValue);
                
            return data.ToArray();
        }
        
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
        
        public static byte[] ReadWrapped(this Stream stream)
        {
            int size = stream.Read(4).ToInt32();
            
            byte[] data = new byte[size];
            int index = 0;
            while((index += stream.Read(data, index, size - index)) < size)
                ;
            
            return data;
        }
        public static void WriteWrapped(this Stream stream, byte[] data)
        {
            stream.Write(data.Length.GetBytes());
            stream.Write(data);
        }
        
        public static byte[] Read(this Stream stream, int length)
        {
            byte[] data = new byte[length];
            
            stream.Read(data, 0, length);
            
            return data;
        }
    }   
}