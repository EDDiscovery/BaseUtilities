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
using System.IO;
using System.Net;
using System.Text;

namespace BaseUtils.WebServer
{
    // a Node which serves files from the filesystem

    public class HTTPFileNode : IHTTPNode
    {
        private string path;

        public HTTPFileNode(string pathbase)
        {
            this.path = pathbase;
        }

        public virtual NodeResponse Response(string partialpath, HttpListenerRequest request)
        {
            string file = Path.Combine(path, partialpath);

            if ( File.Exists(file))
            {
                System.Diagnostics.Debug.WriteLine("File Request: " + file);

                try
                {
                    string ext = Path.GetExtension(file);

                    var content = Server.GetContentType(partialpath);

                    if ( content.Item2)
                    { 
                       // System.Diagnostics.Debug.WriteLine("Transfer " + file + " as binary");
                        return new NodeResponse(File.ReadAllBytes(file), content.Item1);
                    }
                    else
                    {
                        string data = File.ReadAllText(file, Encoding.UTF8);
                        return new NodeResponse( Encoding.UTF8.GetBytes(data), content.Item1);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Swallow file io web exception " + e);
                }
            }

            System.Diagnostics.Debug.WriteLine("File Request: Not found: " + file);
            string text = "Resource not available " + request.Url + " local " + file + Environment.NewLine;
            return new NodeResponse(Encoding.UTF8.GetBytes(text), "text/plain");
        }
    }
}
