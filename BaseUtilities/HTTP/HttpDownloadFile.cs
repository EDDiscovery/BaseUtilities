﻿/*
 * Copyright © 2019-2020 EDDiscovery development team
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
using System.Reflection;

namespace BaseUtils
{
    public static class DownloadFile
    {
        public static string UserAgent { get; set; } = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().FullName.Split(',')[1].Split('=')[1];

        static public bool HTTPDownloadFile(string url,
                                        string filename,                                // if non null, store filename
                                        bool alwaysnewfile,                             // if true, etag is not used, always downloaded 
                                        out bool newfile,                               // returns if new file if storing file
                                        Action<bool, Stream> externalprocessor = null,  // processor, gets newfile and the stream. 
                                        Func<bool> cancelRequested = null,              // cancel requestor
                                        Action<long,double> reportProgress = null)      // report of count and b/s
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
            request.UserAgent = UserAgent;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (etagFilename != null && File.Exists(etagFilename) && File.Exists(filename))   // if we want to etag it, and we have it, and we have the file, we can do a check
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
                            System.Diagnostics.Trace.WriteLine($"HTTP Begin download to {tmpFilename}");

                            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                            long lastreportime = 0;

                            byte[] buffer = new byte[64 * 1024];
                            long count = 0;
                            do
                            {
                                int numread = httpStream.Read(buffer, 0, buffer.Length);        // read in blocks

                                count += numread;

                                var tme = sw.ElapsedMilliseconds;

                                if ( numread == 0 || tme - lastreportime >= 1000)       // if at end, or over a second..
                                {
                                    double rate = count / (tme / 1000.0);
                                    reportProgress?.Invoke(count, rate);
                                    System.Diagnostics.Debug.WriteLine($"{tme} HTTP Downloaded {count:N0} at {rate:N2} b/s");
                                    lastreportime = (tme / 1000) * 1000;
                                }

                                if (numread > 0)
                                {
                                    destFileStream.Write(buffer, 0, numread);

                                    if ( cancelRequested != null && cancelRequested() )
                                    {
                                        destFileStream.Close();
                                        File.Delete(tmpFilename);
                                        return false;
                                    }
                                }
                                else
                                    break;
                            } while (true);
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


