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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace BaseUtils.WebServer
{
    public interface IJSONNode
    {
        JToken Response(string key, JToken message, HttpListenerRequest request);
    }

    // this gets a Request and dispatches it to a node..

    public class JSONDispatcher
    {
        private Dictionary<string, IJSONNode> nodes;
        public string RequestID { get; set; } = "requesttype";      // JSON field of key to look at 
        
        public JSONDispatcher()
        {
            nodes = new Dictionary<string, IJSONNode>();
        }

        public void Add(string node, IJSONNode disp)
        {
            nodes[node] = disp;
        }

        // receive the request, find the node, dispatch, else moan
        
        public JToken Response(HttpListenerRequest request, string key, JToken json)
        {
            if (nodes.ContainsKey(key))
            {
                return nodes[key].Response(key,json,request);
            }
            else
                return null;
        }

        // this one handles it from a websocket and sends the JSON response backYour 

        public void Response(HttpListenerRequest initialwsrequest, WebSocket ws, WebSocketReceiveResult rr, byte[] receiveBuffer, Object lrdata)
        {
            string s = Encoding.UTF8.GetString(receiveBuffer, 0, rr.Count); // bytes, in UTF8, as defined by RFC 6455 section 5.6

            try
            {
                JToken jk = JToken.Parse(s);
                string req = jk[RequestID].StrNull();

                if (req != null)
                {
                    JToken res = Response(initialwsrequest, req, jk);

                    if (res != null)
                    {
                        string ret = res.ToString(Newtonsoft.Json.Formatting.None);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ret);         // to UTF8
                        ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WEBSOCKET Error No responder for "  + req);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Incoming JSON error " + e);
            }
        }
    }

}
