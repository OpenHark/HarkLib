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
        
        public static byte[] ReadUntilTimeout(this Stream stream, int timeout = 0, bool firstByteBlocking = false)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                if(firstByteBlocking)
                {
                    stream.ReadTimeout = System.Threading.Timeout.Infinite;
                    ms.WriteByte((byte)stream.ReadByte());
                }
                
                if(timeout > 0)
                    stream.ReadTimeout = timeout;
                
                try
                {
                    stream.CopyTo(ms);
                }
                catch(System.IO.IOException)
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
        public static byte[] ReadUntil(this Stream stream, byte[] limitValues, bool included = false)
        {
            if(limitValues.Length == 0)
                throw new ArgumentNullException("limitValues");
            
            List<byte> data = new List<byte>();
            
            while(true)
            {
                int value = stream.ReadByte();
                
                if(value == -1)
                    break;
                
                if(value == limitValues[limitValues.Length - 1] && data.Count >= limitValues.Length - 1)
                {
                    bool isFound = true;
                    
                    for(int i = 0; i < limitValues.Length - 1; ++i)
                        if(data[data.Count - limitValues.Length + i + 1] != limitValues[i])
                        {
                            isFound = false;
                            break;
                        }
                    
                    if(isFound)
                    {
                        if(!included)
                            data.RemoveRange(data.Count - limitValues.Length + 1, limitValues.Length - 1);
                        else
                            data.Add((byte)value);
                        break;
                    }
                }
                
                data.Add((byte)value);
            }
            
            return data.ToArray();
        }
        public static string ReadUntil(this Stream stream, string limitValue, bool included = false)
        {
            return stream.ReadUntil(limitValue.GetBytes(), included).GetString();
        }
        public static string ReadUntil(this Stream stream, char limitValue, bool included = false)
        {
            return stream.ReadUntil(new byte[] { (byte)limitValue }, included).GetString();
        }
        
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
        
        public static byte[] ReadWrapped(this Stream stream)
        {
            int size = stream.Read(4).ToInt32();
            
            byte[] data = new byte[size];
            
            if(size == 0)
                return data;
            
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
        
        public static HarkLib.Core.SplitStream[] Split(this Stream stream, int nb)
        {
            return new HarkLib.Core.StreamSplitter(stream, nb).Streams;
        }
    }   
}