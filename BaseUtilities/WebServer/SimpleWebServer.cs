using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

namespace BaseUtils.Web
{
    public class SimpleWebServer
    {
        private readonly HttpListener listener = new HttpListener();
        private readonly Func<HttpListenerRequest, Object, byte[]> responderMethod;
        private Object objectdata;

        // thanks to https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server

        public SimpleWebServer(string[] prefixes, Object data, Func<HttpListenerRequest, Object, byte[]> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                listener.Prefixes.Add(s);

            objectdata = data;
            responderMethod = method;
            listener.Start();
        }

        public SimpleWebServer(Func<HttpListenerRequest, Object, byte[]> method, Object data, params string[] prefixes) : this(prefixes, data, method)
        { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                System.Diagnostics.Debug.WriteLine("Webserver running...");
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c)=> ProcessEvent(c), listener.GetContext()); // GetContext blocks until an event c is ready
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        private void ProcessEvent(Object c)
        {
            var ctx = c as HttpListenerContext;

            try
            {
                System.Diagnostics.Debug.WriteLine("Response");

                byte[] buf = responderMethod(ctx.Request, objectdata);

                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch { } // suppress any exceptions
            finally
            {
                // always close the stream
                ctx.Response.OutputStream.Close();
            }
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }
    }


}



