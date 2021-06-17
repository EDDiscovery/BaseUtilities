/*
 * Copyright © 2019 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
 
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

// thanks to https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server for some of the initial inspiration

namespace BaseUtils.WebServer
{
    public class Server
    {
        public Action<string> ServerLog { get; set; } = null;   // set to get server log messages
        public int WebSocketMsgBufferSize { get; set; } = 8192; // set max size of received packet

        public delegate NodeResponse HTTPResponderFunc(HttpListenerRequest lr, Object lrdata);    // template for HTTP responders
        public delegate void WebSocketsResponderFunc(HttpListenerRequest lr, WebSocket ws,  // template for web socket responders
                                        WebSocketReceiveResult res, byte[] buffer, Object lrdata);

        // open server on these http or https prefixes

        public Server(params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException( "Needs Windows XP SP2, Server 2003 or later.");

            if (prefixes == null || prefixes.Length == 0) // URI prefixes are required, for example http://localhost:8080/index/
                throw new ArgumentException("prefixes");

            foreach (string s in prefixes)
                listener.Prefixes.Add(s);

            prefixesString = string.Join(";", prefixes);

            tks = new CancellationTokenSource();


        }

        // call to add a HTTP responder
        public void AddHTTPResponder(HTTPResponderFunc f, Object data)      
        {
            httpresponder = new Tuple<HTTPResponderFunc, Object>(f, data);
        }

        // call to add a Websockets responder which handles this subprotocol
        public void AddWebSocketsResponder(string subprotocol, WebSocketsResponderFunc f, Object data)      
        {
            websocketresponders.Add(subprotocol, new Tuple<WebSocketsResponderFunc, Object>(f, data));
        }

        //stop - does not wait for thread pools to terminate
        public void Stop()
        {
            tks.Cancel();
        }

        // call this to run the system - returns after kicking it off
        public bool Run()
        {
            try
            {
                listener.Start();
            }
            catch
            {
                return false;       // probably due to a security exception
            }

            ThreadPool.QueueUserWorkItem((o) =>         // in a thread pool, handing incoming requests
            {
                System.Diagnostics.Debug.WriteLine("Listening on " + prefixesString);

                try
                {
                    AsyncCallback OnContextReceived = new AsyncCallback((res) => 
                    {
                        if (!tks.IsCancellationRequested)       // make sure we are not in cancellation mode
                        {
                            HttpListenerContext ctx = listener.EndGetContext(res);

                            if (ctx.Request.IsWebSocketRequest)
                            {
                                string protocol = ctx.Request.Headers["Sec-WebSocket-Protocol"]; // null if not there

                                if (protocol != null && websocketresponders.Count > 0 && websocketresponders.ContainsKey(protocol))    // must have a protocol (even if null)
                                {
                                    System.Diagnostics.Debug.WriteLine("WEBSOCKET Requesting protocol " + protocol);
                                    //System.Diagnostics.Debug.WriteLine("Headers:" + ctx.Request.RequestHeaders().LineTextInsersion("  "));

                                    // a new thread handles the web sockets
                                    ThreadPool.QueueUserWorkItem((ox) => { ProcessWebSocket(ctx, protocol, websocketresponders[protocol], tks.Token); });  
                                }
                                else
                                {
                                    ctx.Response.StatusCode = 400;
                                    ctx.Response.Close();
                                }
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine("HTTP: " + ctx.Request.Url + " " + ctx.Request.UserHostName + " : " + ctx.Request.RawUrl);
                                //System.Diagnostics.Debug.WriteLine("Headers:" + ctx.Request.RequestHeaders().LineTextInsersion("  "));

                                if (httpresponder != null)
                                {
                                    ProcessHTTP(ctx);       // by not shelling out to a thread per HTTP request, we are serialising them for now... should have no impact
                                                            // do this for another method.. ThreadPool.QueueUserWorkItem((c) => ProcessHTTP((HttpListenerContext)c),ctx); // throw handling into a thread 
                                }
                                else
                                {
                                    ctx.Response.StatusCode = 400;
                                    ctx.Response.Close();
                                }
                            }
                        }
                    });

                    while (!tks.IsCancellationRequested)            // sit here, spawing off a begin get context (which OnContextReceived services) 
                                                                    // and wait for either a completion or a cancellation
                    {
                        //System.Diagnostics.Debug.WriteLine("beinggetcontext");
                        IAsyncResult iar=  listener.BeginGetContext(OnContextReceived, null);
                        //System.Diagnostics.Debug.WriteLine("wait");
                        WaitHandle.WaitAny(new WaitHandle[] { iar.AsyncWaitHandle, tks.Token.WaitHandle });
                        //System.Diagnostics.Debug.WriteLine("wait over");
                    }

                    System.Diagnostics.Debug.WriteLine("listener abort");
                    listener.Abort();
                }
                catch ( Exception e )
                {
                    System.Diagnostics.Debug.WriteLine("Exception " + e);

                } // suppress any exceptions

                System.Diagnostics.Debug.WriteLine("Listening close");
                listener.Close();
                listener = null;
                System.Diagnostics.Debug.WriteLine("Listening now null");

            });

            return true;
        }

        // call to send text (JSON normally) to websocket.
        public bool SendWebSockets(string text, bool wait = false)
        {
            bool okay = true;

            lock( webSockets)        // we are deep in multithread do-do, lock so we don't get caught out.
            {
                foreach (var c in webSockets)
                {
                    try
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);         // to UTF8

                        var tsk = c.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        if (wait)
                            tsk.Wait();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Web socket send exception " + e);
                        okay = false;
                    }
                }
            }

            return okay;
        }

        // send JSON
        public bool SendWebSockets(BaseUtils.JSON.JToken json, bool wait = false)
        {
            return SendWebSockets(json.ToString(), wait);
        }

        #region Implementation

        // an event from the listeners has happened which is HTTP, process it..

        private void ProcessHTTP(HttpListenerContext ctx)
        {
            try
            {
                ServerLog?.Invoke(ctx.Request.RequestInfo());

                NodeResponse res = httpresponder.Item1(ctx.Request, httpresponder.Item2);      // get response from method.  Always responds with data
                ctx.Response.ContentType = res.ContentType;
                ctx.Response.ContentLength64 = res.Data.Length;
                if ( res.Headers != null )
                    ctx.Response.Headers.Add(res.Headers);

#if false
                var x = ctx.Response.Headers.AllKeys;

                WebHeaderCollection headers = ctx.Response.Headers;
                foreach (string key in headers.AllKeys)
                {
                    string[] values = headers.GetValues(key);
                    if (values.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("The values of the " + key + " header are: ");
                        foreach (string value in values)
                        {
                            System.Diagnostics.Debug.WriteLine("   " + value);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("There is no value associated with the header.");
                    }
                }
#endif
                ctx.Response.OutputStream.Write(res.Data, 0, res.Data.Length);
            }
            catch ( Exception e)
            {   // suppress any exceptions
                System.Diagnostics.Debug.WriteLine("Process HTTP exception " + e);
            } 
            finally
            {
                // always close response - this sends the response back to the client
                ctx.Response.Close();
            }
        }

        // a web socket is attaching
        private async void ProcessWebSocket(HttpListenerContext ctx, string protocol , Tuple<WebSocketsResponderFunc, Object> responder, CancellationToken canceltk)
        {
            WebSocketContext wsctx;

            try
            {
                System.Diagnostics.Debug.WriteLine("WEBSOCKET Accepting on " + prefixesString + " protocol " + protocol);
                wsctx = await ctx.AcceptWebSocketAsync(protocol);
            }
            catch (Exception e)
            {
                // The upgrade process failed somehow. For simplicity lets assume it was a failure on the part of the server and indicate this using 500.
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e);
                return;
            }

            ServerLog?.Invoke("WEBSOCKET accepted " + prefixesString );
            System.Diagnostics.Debug.WriteLine("WEBSOCKET accepted " + prefixesString);

            WebSocket webSocket = wsctx.WebSocket;

            lock (webSockets)
                webSockets.Add(webSocket);       // add to the pool of web sockets to throw data at.


            try
            {
                byte[] receiveBuffer = new byte[WebSocketMsgBufferSize];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), canceltk);

                    ServerLog?.Invoke("WEBSOCKET request " + prefixesString + ": " + receiveResult.MessageType);
                    System.Diagnostics.Debug.WriteLine("WEBSOCKET request " + prefixesString + " type " + receiveResult.MessageType + " len " + receiveResult.Count);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        System.Diagnostics.Debug.WriteLine("WEBSOCKET close req " + prefixesString);
                        webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();        // here we block until complete
                    }
                    else
                    {
                        responder.Item1(ctx.Request, webSocket, receiveResult, receiveBuffer, responder.Item2);
                    }
                }

                System.Diagnostics.Debug.WriteLine("WEBSOCKET closed on " + prefixesString);
            }
            catch(System.Threading.Tasks.TaskCanceledException)
            {
                // normal task canceled exception
            }
            catch (Exception e)
            {
                // Just log any exceptions to the console. Pretty much any exception that occurs when calling `SendAsync`/`ReceiveAsync`/`CloseAsync` is unrecoverable in that it will abort the connection and leave the `WebSocket` instance in an unusable state.
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e);
            }
            finally
            {
                if (webSocket != null)      // Clean up by disposing the WebSocket once it is closed/aborted.
                {
                    lock (webSockets)
                        webSockets.Remove(webSocket);
                    webSocket.Dispose();
                }
            }

            System.Diagnostics.Debug.WriteLine("WEBSOCKET terminate " + prefixesString);
        }

#endregion

        public static string[] BinaryMIMETypes { get; set; } = new string[] { "image/" };

        public static Tuple<string,bool> GetContentType(string path)
        {
            string contenttype = System.Web.MimeMapping.GetMimeMapping(path);
            bool readbin = BinaryMIMETypes.StartsWith(contenttype, StringComparison.InvariantCultureIgnoreCase) >= 0;

            if (contenttype == "application/x-javascript")
                contenttype = "text/javascript";        // per https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types

            System.Diagnostics.Debug.WriteLine("File {0} = {1} bin {2}", path, contenttype, readbin);

            return new Tuple<string, bool>(contenttype, readbin);
        }

#region vars

        protected HttpListener listener = new HttpListener();

        private Tuple<HTTPResponderFunc, Object> httpresponder = null;      // http requests go thru this function for service

        private List<WebSocket> webSockets = new List<WebSocket>();

                                                                            // websocket requests, for protocol string, go thru this function for service
        private Dictionary<string, Tuple<WebSocketsResponderFunc, Object>> websocketresponders = new Dictionary<string, Tuple<WebSocketsResponderFunc, object>>();

        protected string prefixesString;    // debug only

        CancellationTokenSource tks;        // for graceful shut down
#endregion

    }

}



