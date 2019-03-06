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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace BaseUtils
{
    // this is the non stomach churning version of HTTP download that you can understand

    public static class DownloadFile
    {
        static public bool HTTPDownloadFile(string url,
                                        string filename,                                // if non null, store filename
                                        bool alwaysnewfile,                             // if true, etag is not used, always downloaded 
                                        out bool newfile,                               // returns if new file if storing file
                                        Action<bool, Stream> externalprocessor = null,  // processor, gets newfile and the stream. 
                                        Func<bool> cancelRequested = null)              // cancel requestor
        {
            newfile = false;

            System.Diagnostics.Debug.Assert(filename != null || externalprocessor != null);
            System.Diagnostics.Debug.Assert(url != null);

            if (!url.Contains("http"))
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Not valid url (Debug) {0} ", url));
                return false;
            }

            var etagFilename = filename == null || alwaysnewfile ? null : filename + ".etag";           // if we give a filename and we are not always asking for a new file, we get a .etag file

            BaseUtils.HttpCom.WriteLog("DownloadFile", url + ": ->" + filename + ":" + etagFilename);

            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.UserAgent = BrowserInfo.UserAgent;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (etagFilename != null && File.Exists(etagFilename))                                      // if we want to etag it, and we have it
            {
                var etag = File.ReadAllText(etagFilename);
                if (etag != "")
                    request.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                else
                    request.IfModifiedSince = File.GetLastWriteTimeUtc(etagFilename);
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                BaseUtils.HttpCom.WriteLog("Response", response.StatusCode.ToString());

                if (cancelRequested?.Invoke() == true)
                    return false;

                using (var httpStream = response.GetResponseStream())
                {
                    newfile = true;

                    if (filename != null)               // if write to file..
                    {
                        var tmpFilename = filename + ".tmp";        // copy thru tmp
                        using (var destFileStream = File.Open(tmpFilename, FileMode.Create, FileAccess.Write))
                        {
                            httpStream.CopyTo(destFileStream);
                        }

                        File.Delete(filename);
                        File.Move(tmpFilename, filename);

                        if (etagFilename != null)
                        {
                            File.WriteAllText(etagFilename, response.Headers[HttpResponseHeader.ETag]);
                            File.SetLastWriteTimeUtc(etagFilename, response.LastModified.ToUniversalTime());
                        }

                        if ( externalprocessor != null )
                        {
                            using (var cacheStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                externalprocessor.Invoke(true, cacheStream);            // let the external processor see it
                            }
                        }
                    }
                    else if ( externalprocessor != null)
                    {
                        externalprocessor.Invoke(true, httpStream);            // let the external processor see it
                    }
                }

                return true;
            }
            catch (WebException ex)
            {
                if ((HttpWebResponse)ex.Response == null)
                    return false;

                if (cancelRequested?.Invoke() == true)
                    return false;

                var code = ((HttpWebResponse)ex.Response).StatusCode;

                if (code == HttpStatusCode.NotModified)
                {
                    System.Diagnostics.Trace.WriteLine("DownloadFile: " + filename + " up to date (etag).");
                    BaseUtils.HttpCom.WriteLog(filename, "up to date (etag).");

                    if (externalprocessor != null)          // feed thru processor..
                    {
                        using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            externalprocessor.Invoke(false, stream);
                        }
                    }

                    return true;
                }

                System.Diagnostics.Trace.WriteLine("DownloadFile Exception:" + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                BaseUtils.HttpCom.WriteLog("Exception", ex.Message);
            }
            catch (Exception ex)
            {
                if (cancelRequested?.Invoke() == true)
                    return false;

                System.Diagnostics.Trace.WriteLine("DownloadFile Exception:" + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                BaseUtils.HttpCom.WriteLog("DownloadFile Exception", ex.Message);
            }

            return false;
        }
    }
}


