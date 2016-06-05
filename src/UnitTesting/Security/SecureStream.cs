using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Security;
using System.Linq;
using System.Net;
using System.IO;
using System;

using HarkLib.Security;

namespace UnitTesting.Security
{
    public class SecuritySecureStream : ITest
    {
        public SecuritySecureStream()
        {
            DoneEvents = new ManualResetEvent[]
            {
                new ManualResetEvent(false),
                new ManualResetEvent(false)
            };
        }
        
        public override string Name
        {
            get
            {
                return "Security.SecureStream";
            }
        }
        
        protected ManualResetEvent[] DoneEvents
        {
            get;
            private set;
        }
        
        protected TcpListener server
        {
            get;
            private set;
        }
        
        protected string ServerReception
        {
            get;
            set;
        }
        
        protected string ClientReception
        {
            get;
            set;
        }
        
        protected void Server(Object threadContext)
        {
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            
            SecureStream ss = new SecureStream(stream);
            ss.WriteWrapped("Hello friend, here is the server!".GetBytes());
            byte[] receivedData = ss.ReadWrapped();
            ServerReception = receivedData.GetString();
            
            if(IsVerbose)
                Console.WriteLine("Server : " + ServerReception);
            
            DoneEvents[(int)threadContext].Set();
        }
        
        protected void Client(Object threadContext)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 65001));
            NetworkStream stream = client.GetStream();
            
            SecureStream ss = new SecureStream(stream);
            ss.WriteWrapped("Hello friend, here is the client!".GetBytes());
            byte[] receivedData = ss.ReadWrapped();
            ClientReception = receivedData.GetString();
            
            if(IsVerbose)
                Console.WriteLine("Client : " + ClientReception);
            
            DoneEvents[(int)threadContext].Set();
        }
        
        public override bool Execute()
        {
            this.server = new TcpListener(IPAddress.Loopback, 65001);
            server.Start();
            
            ThreadPool.QueueUserWorkItem(Server, 0);
            ThreadPool.QueueUserWorkItem(Client, 1);
            
            WaitHandle.WaitAll(DoneEvents);
            
            return  ServerReception == "Hello friend, here is the client!" &&
                    ClientReception == "Hello friend, here is the server!";
        }
    }
}