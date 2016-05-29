using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System;

namespace HarkLib.Net
{
    public class WebServer
    {
        public WebServer(int port = 80, IPAddress localAddress = null)
        {
            this.LocalAddress = localAddress ?? IPAddress.Loopback;
            this.Methods = new List<Method>();
            this.Port = port;
            
            this.AddProviders(this);
        }
        
        public abstract class Method
        {
            public Method(MethodInfo m, object caller)
            {
                this.Information = m
                    .GetCustomAttributes(typeof(WebServerMethod), true)
                    .Cast<WebServerMethod>()
                    .First();
                
                this.Caller = caller;
            }
            
            public object Caller
            {
                get;
                private set;
            }
            
            public WebServerMethod Information
            {
                get;
                private set;
            }
        }
        public class BytesMethod : Method
        {
            BytesMethod(MethodInfo m, object caller)
                : base(m, caller)
            { }
        }
        
        [System.Serializable]
        public class WrongMethodTypeException : System.Exception
        {
            public WrongMethodTypeException() { }
            public WrongMethodTypeException( string message ) : base( message ) { }
            public WrongMethodTypeException( string message, System.Exception inner ) : base( message, inner ) { }
            protected WrongMethodTypeException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
        }
        
        public Method GetMethod(MethodInfo m, object provider)
        {
            if(m.ReturnType == typeof(void))
                return null;
            
            throw new WrongMethodTypeException();
        }
        
        public List<Method> Methods
        {
            get;
            private set;
        }
        
        public void AddProviders(object provider)
        {
            provider
                .GetType()
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(WebServerMethod), true).Length > 0)
                .Select(m => GetMethod(m, provider))
                .To(Methods.AddRange);
        }
        
        private TcpListener server = null;
        
        public int Port
        {
            get;
            set;
        }
        
        public IPAddress LocalAddress
        {
            get;
            set;
        }
        
        public void Start(int port = -1, IPAddress localAddress = null)
        {
            if(server != null)
                throw new Exception("The server looks to be already started.");
            
            try
            {
                server = new TcpListener(localAddress ?? LocalAddress ?? IPAddress.Loopback, port <= 0 ? Port : port);
                server.Start();
                
                new Thread(new ThreadStart(ServerRuntime)).Start();
            }
            catch
            {
                server = null;
                throw;
            }
        }
        
        protected virtual void ServerRuntime()
        {
            while(true)
            {
                Socket client = server.AcceptSocket();
                
                if(client.Connected)
                    new Thread(new ParameterizedThreadStart(ClientRuntime)).Start(client);
            }
        }
        
        protected virtual void ClientRuntime(object oClient)
        {
            Socket client = (Socket)oClient;
            
            // Methods.Where(m => m.Matches())
        }
    }
}