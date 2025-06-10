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
 *
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BaseUtils.WebServer
{
    // this is the data passed back to send, and other parameters
    public class NodeResponse
    {
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public WebHeaderCollection Headers { get; set; }

        public NodeResponse(byte[] d, string ct, WebHeaderCollection hc = null)
        {
            Data = d;
            ContentType = ct;
            Headers = hc;
        }
    }

    public interface IHTTPNode
    {
        NodeResponse Response(string partialpath, HttpListenerRequest request);       // if return null, you get a resource unavailable message back
    }

    // this holds a list of http dispatchers and processes the HTTPlistenerrequest and decides which one to use

    public class HTTPDispatcher
    {
        private Dictionary<string, IHTTPNode> terminalnodes;
        private List<Tuple<string, IHTTPNode>> partialpathnodes;
        
        public IHTTPNode URLNotFound { get; set; }
        public string RootNodeTranslation = null;

        public HTTPDispatcher()
        {
            terminalnodes = new Dictionary<string, IHTTPNode>();
            partialpathnodes = new List<Tuple<string, IHTTPNode>>();
        }

        public void AddTerminalNode(string node, IHTTPNode disp)
        {
            terminalnodes[node] = disp;
        }

        public void AddPartialPathNode(string node, IHTTPNode disp)
        {
            partialpathnodes.Add(new Tuple<string, IHTTPNode>(node, disp));
        }

        // receive the request, find the node, dispatch, else moan

        public NodeResponse Response(HttpListenerRequest request)
        {
            string resourcepath = request.Url.AbsolutePath;

            if (resourcepath == "/" && RootNodeTranslation != null)
                resourcepath = RootNodeTranslation;

            if (terminalnodes.ContainsKey(resourcepath))
            {
                var r = terminalnodes[resourcepath].Response(resourcepath, request);
                if (r != null)
                    return r;
            }
            else
            {
                var disp = partialpathnodes.Find(x => resourcepath.StartsWith(x.Item1));
                if (disp != null)
                {
                    var r = disp.Item2.Response(resourcepath.Substring(disp.Item1.Length), request);
                    if (r != null)
                        return r;
                }
            }

            if (URLNotFound != null)
                return URLNotFound.Response(resourcepath, request);

            string notfound = "Resource not available " + request.Url;
            return new NodeResponse(Encoding.UTF8.GetBytes(notfound),"text/plain");
        }
    }

}
