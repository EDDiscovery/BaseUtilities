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
        public Action<string> ServerLog { get; set; } = null;   // set to get server log info
        public int MsgBufferSize { get; set; } = 8192;          // set max size of received packet
               

        public delegate byte[] HTTPResponderFunc(HttpListenerRequest lr, Object lrdata);    // template for HTTP responders
        public delegate void WebSocketsResponderFunc(HttpListenerRequest lr, WebSocket ws,  // template for web socket responders
                                        WebSocketReceiveResult res, byte[] buffer, Object lrdata);

        WebSocketState SocketState() { return webSocketContext?.WebSocket.State ?? WebSocketState.None; }

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
        }

        // call to add a HTTP responder
        public void AddHTTPResponder(HTTPResponderFunc f, Object data)      
        {
            httpresponder = new Tuple<HTTPResponderFunc, Object>(f, data);
        }

        // call to add a Websockets responder which handles this subprotocol
        public void AddWebServerResponder(string subprotocol, WebSocketsResponderFunc f, Object data)      
        {
            websocketresponders.Add(subprotocol, new Tuple<WebSocketsResponderFunc, Object>(f, data));
        }

        //stop
        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }

        // call this to run the system - returns after kicking it off
        public void Run()
        {
            System.Diagnostics.Debug.WriteLine("Listening on " + prefixesString);
            listener.Start();

            ThreadPool.QueueUserWorkItem((o) =>         // in a thread pool, handing incoming requests
            {
                try
                {
                    while (listener.IsListening)
                    {
                        HttpListenerContext ctx = listener.GetContext();    // block, get

                        System.Diagnostics.Debug.WriteLine(ctx.Request.RequestHeaders());

                        if (ctx.Request.IsWebSocketRequest)
                        {
                            string protocol = ctx.Request.Headers["Sec-WebSocket-Protocol"]; // null if not there

                            if ( protocol != null && websocketresponders.Count > 0 && websocketresponders.ContainsKey(protocol))    // must have a protocol (even if null)
                            {
                                System.Diagnostics.Debug.WriteLine("Requesting protocol " + protocol);
                                ThreadPool.QueueUserWorkItem((ox) => { ProcessWebSocket(ctx, protocol, websocketresponders[protocol]); });  // throw handling into a thread pool so we don't block this threa
                            }
                            else
                            {
                                ctx.Response.StatusCode = 400;
                                ctx.Response.Close();
                            }
                        }
                        else
                        {
                            if ( httpresponder != null )
                            {
                                ProcessHTTP(ctx);       // by not shelling out to a thread per HTTP request, we are serialising them for now
                                //ThreadPool.QueueUserWorkItem((c) => ProcessHTTP((HttpListenerContext)c),ctx); // throw handling into a thread 
                            }
                            else
                            {
                                ctx.Response.StatusCode = 400;
                                ctx.Response.Close();
                            }
                        }

                    }
                }
                catch ( Exception e )
                {
                    System.Diagnostics.Debug.WriteLine("Exception " + e);

                } // suppress any exceptions
            });
        }

        public bool SendWebSocket(string text, bool wait = false)
        {
            if (webSocketContext != null)
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);         // to UTF8

                    var tsk = webSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    if (wait)
                        tsk.Wait();

                    return true;
                }
                catch( Exception e )
                {
                    System.Diagnostics.Debug.WriteLine("Web socket send exception " + e);
                }
            }

            return false;
        }

        #region Implementation

        // an event from the listeners has happened which is HTTP, process it..

        private void ProcessHTTP(HttpListenerContext ctx)
        {
            System.Diagnostics.Debug.WriteLine("HTTP Req " + ctx.Request.UserHostName + " : " + ctx.Request.RawUrl);

            try
            {
                ServerLog?.Invoke(ctx.Request.RequestInfo());

                byte[] buf = httpresponder.Item1(ctx.Request, httpresponder.Item2);      // get response from method.

                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
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
        private async void ProcessWebSocket(HttpListenerContext ctx, string protocol , Tuple<WebSocketsResponderFunc, Object> responder)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("WS Accepting on " + prefixesString + " protocol " + protocol);
                webSocketContext = await ctx.AcceptWebSocketAsync(protocol);
            }
            catch (Exception e)
            {
                // The upgrade process failed somehow. For simplicity lets assume it was a failure on the part of the server and indicate this using 500.
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e);
                webSocketContext = null;
                return;
            }

            ServerLog?.Invoke("WS connect " + prefixesString );
            System.Diagnostics.Debug.WriteLine("WS Receive loop on " + prefixesString);
            WebSocket webSocket = webSocketContext.WebSocket;

            try
            {
                byte[] receiveBuffer = new byte[MsgBufferSize];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    ServerLog?.Invoke("WS rx " + prefixesString + ": " + receiveResult.MessageType);

                    System.Diagnostics.Debug.WriteLine("WS rx " + prefixesString + " type " + receiveResult.MessageType + " len " + receiveResult.Count);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        System.Diagnostics.Debug.WriteLine("WS close req " + prefixesString);
                        webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();        // here we block until complete
                    }
                    else
                    {
                        responder.Item1(ctx.Request, webSocket, receiveResult, receiveBuffer, responder.Item2);
                    }
                }

                System.Diagnostics.Debug.WriteLine("WS closed on " + prefixesString);

            }
            catch (Exception e)
            {
                // Just log any exceptions to the console. Pretty much any exception that occurs when calling `SendAsync`/`ReceiveAsync`/`CloseAsync` is unrecoverable in that it will abort the connection and leave the `WebSocket` instance in an unusable state.
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e);
            }
            finally
            {
                if (webSocket != null)      // Clean up by disposing the WebSocket once it is closed/aborted.
                    webSocket.Dispose();
            }

            webSocketContext = null;
            System.Diagnostics.Debug.WriteLine("terminate Websocket " + prefixesString);
        }

        #endregion

        #region vars

        protected HttpListener listener = new HttpListener();

        private Tuple<HTTPResponderFunc, Object> httpresponder = null;

        private WebSocketContext webSocketContext = null;
        private Dictionary<string, Tuple<WebSocketsResponderFunc, Object>> websocketresponders = new Dictionary<string, Tuple<WebSocketsResponderFunc, object>>();

        protected string prefixesString;    // debug only

        #endregion

    }

}



