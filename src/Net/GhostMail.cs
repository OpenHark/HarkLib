using System.Globalization;
using System.Net.Sockets;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace HarkLib.Net
{
    public class GhostMail
    {
        public GhostMail()
        {
            this.ContentType = "text/plain; charset=UTF-8";
            this.DestinationEMail = null;
            this.DestinationUser = null;
            this.DestinationPort = 25;
            this.Date = DateTime.Now;
            this.SourceEMail = null;
            this.SourceUser = null;
            this.Subject = null;
            this.Content = null;
        }
        
        public string DestinationEMail
        {
            get;
            set;
        }
        
        public string DestinationUser
        {
            get;
            set;
        }
        
        public string DestinationHost
        {
            get
            {
                return DestinationEMail.Substring(DestinationEMail.IndexOf('@') + 1);
            }
        }
        public string SourceHost
        {
            get
            {
                return SourceEMail.Substring(SourceEMail.IndexOf('@') + 1);
            }
        }
        
        public string SourceUser
        {
            get;
            set;
        }
        
        public string SourceEMail
        {
            get;
            set;
        }
        
        public string Subject
        {
            get;
            set;
        }
        
        public string Content
        {
            get;
            set;
        }
        
        public int DestinationPort
        {
            get;
            set;
        }
        
        public DateTime Date
        {
            get;
            set;
        }
        
        public string ContentType
        {
            get;
            set;
        }
        
        protected int WaitForResultCode(Stream stream)
        {
            byte[] bytes = new byte[1024];
            stream.Read(bytes, 0, 1024);
            
            byte[] bcode = new byte[]
            {
                bytes[0],
                bytes[1],
                bytes[2]
            };
            return int.Parse(bcode.GetString());
        }
        
        public string GetMessage()
        {
            return "From: \"" + SourceUser + "\" <" + SourceEMail + ">\r\n" +
                "To: \"" + DestinationUser + "\" <" + DestinationEMail + ">\r\n" +
                "Date: " + Date.ToString("ddd, dd MMM yyyy HH:mm:ss -0500", new CultureInfo("en")) + "\r\n" +
                "MIME-Version: 1.0\r\n" +
                "Content-type: " + ContentType + "\r\n" +
                "Subject: " + Subject + "\r\n" + "\r\n" + Content;
        }
        
        public void Send()
        {
            this.Content = this.Content ?? "";
            
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPAddress addr = Resolver.GetIPs(Resolver.GetMxRecords(DestinationHost.ToLower()).First()).First();
            
            socket.Connect(addr, DestinationPort);
            
            Stream stream = new NetworkStream(socket);
            
            int code;
            
            if((code = WaitForResultCode(stream)) != 220)
                throw new EMailTransmissionException(code, "CONNECTION " + addr + ":" + DestinationPort);
            
            stream.Write(("EHLO "+SourceHost+"\r\n").GetBytes());
            
            if((code = WaitForResultCode(stream)) != 250)
                throw new EMailTransmissionException(code, "EHLO");
            
            stream.Write(("MAIL FROM:<"+SourceEMail+">\r\n").GetBytes());
            
            if((code = WaitForResultCode(stream)) != 250)
                throw new EMailTransmissionException(code, "MAIL");
            
            stream.Write(("RCPT TO:<"+DestinationEMail+">\r\n").GetBytes());
            
            if((code = WaitForResultCode(stream)) != 250)
                throw new EMailTransmissionException(code, "RCPT");
            
            stream.Write("DATA\r\n".GetBytes());
            
            string content = GetMessage() + "\r\n.\r\n";
            
            if((code = WaitForResultCode(stream)) != 250)
                throw new EMailTransmissionException(code, "DATA");
            
            stream.Write(content.GetBytes());
            stream.Flush();
            
            if((code = WaitForResultCode(stream)) != 354)
                throw new EMailTransmissionException(code, "DATA-WRITE");
            
            stream.Write("QUIT\r\n".GetBytes());
            
            if((code = WaitForResultCode(stream)) != 250)
                throw new EMailTransmissionException(code, "QUIT");
        }
    }
}