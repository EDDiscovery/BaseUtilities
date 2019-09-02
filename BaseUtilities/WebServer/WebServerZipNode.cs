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
using System.IO.Compression;

namespace BaseUtils.WebServer
{
    public class HTTPZipNode : IHTTPNode
    {
        public string[] BinaryTypes = new string[] { ".png", ".bmp" };

        private string path;

        public HTTPZipNode(string pathbase)
        {
            this.path = pathbase;
        }

        public virtual byte[] Response(string partialpath, HttpListenerRequest request)
        {
            try
            {
                using (var zipfile = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    //foreach (var x in zipfile.Entries) System.Diagnostics.Debug.WriteLine("Zip file " + x.FullName);

                    partialpath = partialpath.Replace('/', '\\');       // zip files use back slashes and urls are forward slashes.

                    var file = zipfile.GetEntry(partialpath);
                    System.Diagnostics.Debug.WriteLine("Request " + partialpath);

                    if (file != null)
                    {
                        using (var zipstrm = file.Open())
                        {
                            var memstrm = new MemoryStream(); // Image will own this
                            zipstrm.CopyTo(memstrm);
                            string ext = Path.GetExtension(partialpath);

                            if (BinaryTypes.ContainsIn(ext, StringComparison.InvariantCultureIgnoreCase) != -1)
                                return memstrm.ToArray();
                            else
                            {
                                memstrm.Position = 0;
                                using (StreamReader sr = new StreamReader(memstrm))     // surprisingly difficult to arrange to work to handle the UTF process
                                {
                                    string s = sr.ReadToEnd();
                                    var data = Encoding.UTF8.GetBytes(s);
                                    return data;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Swallow zip web exception " + e);
            }

            System.Diagnostics.Debug.WriteLine("File Request: Not found: " + partialpath);
            string text = "Resource not available " + request.Url + " local " + partialpath + Environment.NewLine;
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
