/*
 * Copyright © 2016 - 2020 EDDiscovery development team
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
using System.Text;

namespace BaseUtils
{
    public static class HttpUriEncode
    {
        public static string URIGZipBase64Escape(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);

            using (MemoryStream indata = new MemoryStream(bytes))
            {
                using (MemoryStream outdata = new MemoryStream())
                {
                    using (System.IO.Compression.GZipStream gzipStream = new System.IO.Compression.GZipStream(outdata, System.IO.Compression.CompressionLevel.Optimal, true))
                        indata.CopyTo(gzipStream);      // important to clean up gzip otherwise all the data is not written.. using

                    return Uri.EscapeDataString(Convert.ToBase64String(outdata.ToArray()));
                }
            }
        }
    }
}
