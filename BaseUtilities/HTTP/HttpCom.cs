/*
 * Copyright © 2016-2024 EDDiscovery development team
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
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace BaseUtils
{
    public class HttpCom
    {
        // set path to cause logging to occur
        static public string LogPath { get; set; } = null;

        // default user agent is the entry assembly name. Override if you want another, or set to Null not to send
        public string UserAgent { get; set; } 

        // Server address to use, endpoint is added to this
        // can be "" meaning the endpoint is the full URL
        public string ServerAddress { get { return httpserveraddress; } set { System.Diagnostics.Debug.Assert(value != null);  httpserveraddress = value; } }

        public const string DefaultContentType = "application/json; charset=utf-8";
        public const int DefaultTimeout = 20000;

        // backwards compatible setting user agent
        public HttpCom()
        {
            UserAgent = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " v" + System.Reflection.Assembly.GetEntryAssembly().FullName.Split(',')[1].Split('=')[1];
        }

        // backwards compatible setting user agent
        public HttpCom(string server)
        {
            ServerAddress = server;
            UserAgent = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + " v" + System.Reflection.Assembly.GetEntryAssembly().FullName.Split(',')[1].Split('=')[1];
        }

        // Full control
        public HttpCom(string server,string useragent)
        {
            ServerAddress = server;
            UserAgent = useragent;
        }

        public enum Method { POST, GET };

        #region HTTP Requests

        // Blocking POST request, with postdata and with timeout.  Postdata first due to historical reasons
        // Response returned always
        protected Response RequestPost(string postData, string endpoint, NameValueCollection headers = null, string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            return BlockingRequest(Method.POST, endpoint, postData, headers, contenttype,timeout);
        }

        // Blocking GET request, with timeout
        // Headers automatically has Accept-Encoding gzip/deflate added
        // Response returned always
        protected Response RequestGet(string endpoint, NameValueCollection headers = null, string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            if (headers == null)
                headers = new NameValueCollection();

            headers.Add("Accept-Encoding", "gzip,deflate");

            return BlockingRequest(Method.GET, endpoint, "", headers, contenttype, timeout);
        }

        // Blocking request, with timeout. No cancellation
        // method = HTTP method
        // endpoint = end point to access or http full address
        // headerData for POST etc. normally json. Otherwise null.
        // headers = HTTP headers to send, may be null
        // content type must be set
        // return Response Always.  Status code is BadRequest (exception due to data), or server response.

        public Response BlockingRequest(Method method, string endpoint, 
                                        string headerData = null, NameValueCollection headers = null,
                                        string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            try
            {
                try
                {
                    HttpWebRequest request = MakeRequest(method, endpoint, headerData, headers, contenttype);

                    request.Timeout = timeout;      // set the timeout for the GetResponse() 

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var responsedata = Response.Create(response,true);          // always returns a data object, even if response = null;
                    response?.Close();
                    return responsedata;
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    var responsedata = Response.Create(response,true);          // always returns a data object, even if response = null;

                    WriteLog("HTTPCom Response Exception", ex.Message);
                    WriteLog($".. Error code", responsedata.StatusCode.ToString());
                    WriteLog($".. Error body", responsedata.Body);
                    WriteLog($".. stack:", ex.StackTrace);

                    response?.Close();

                    return responsedata;
                }
            }
            catch (Exception ex)
            {
                WriteLog("HTTPCom Request Exception", ex.Message);
                WriteLog("..Stack", ex.StackTrace);

                return new Response(HttpStatusCode.BadRequest);
            }
        }

        // blocking request run in task. return Response.  You can await on this. There is no aborting, timeout will stop it.
        public System.Threading.Tasks.Task<Response> RequestInTask(Method method, string endpoint, 
                                        string headerData = null, NameValueCollection headers = null,
                                        string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return BlockingRequest(method, headerData, endpoint, headers, contenttype, timeout);
            });
        }


        // Use the BeginGetResponse pattern to run a query asynchronously
        // note it may still take time to set up so will block until it launches
        // callback is void RespCallback(Response, object callerdata).  
        // callback will be in a task not the UI thread. You need to transition to UI thread if applicable using BeginInvoke
        // there is no timeout here
        public IAsyncResult AsyncRequest(Action<Response, object> returncallback, object callerdata,
                                        Method method, string endpoint, 
                                        string headerData = null, NameValueCollection headers = null,
                                        string contenttype = DefaultContentType)
        {
            HttpWebRequest request = MakeRequest(method, endpoint, headerData, headers, contenttype);
            return AsyncRequest(returncallback, callerdata, request);
        }

        // Use the BeginGetRepsonse pattern to run a query asynchronously
        // if readbody = true, this function reads the data stream and stores it in rp.Body
        // Because the HttpWebRequest was sent, you can use .Abort() on it externally to stop the request.  Return will be RequestTimeout.
        public IAsyncResult AsyncRequest(Action<Response, object> returncallback, object callerdata, HttpWebRequest request, bool readbody = true)
                                        
        {
            try
            {
                var asyncresult = request.BeginGetResponse(AysncRespCallback, new Tuple<HttpWebRequest, Action<Response, object>, object,bool>(request, returncallback, callerdata,readbody));
                return asyncresult;
            }
            catch (Exception ex)
            {
                WriteLog("HTTPCom Request Async Exception",ex.Message);
                WriteLog(ex.StackTrace);
                return null;
            }
        }

        // run in task using ASYNC
        // You can await on this
        // cancel event allows the request to be aborted.
        // return Response, or null if cancelled, or RequestTimeout if timeout occurred.
        public System.Threading.Tasks.Task<Response> AsyncRequestInTask(CancellationToken cancel, Method method, string endpoint, 
                                        string headerData = null, NameValueCollection headers = null,
                                        string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            HttpWebRequest request = MakeRequest(method, endpoint, headerData, headers, contenttype);
            return AsyncRequestInTask(cancel, request, timeout);
        }

        // run in task using ASYNC
        // You can await on this
        // cancel event allows the request to stops. Or you can call Abort() on the request.
        // return Response, or null if cancelled, or RequestTimeout if timeout occurred.
        public System.Threading.Tasks.Task<Response> AsyncRequestInTask(CancellationToken cancel, HttpWebRequest request, int timeout)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                return BlockingRequest(cancel, request, timeout);
            });
        }

        // perform a BlockingRequest but with a cancellation token.
        // Wait for it to complete, or a timeout. Return Response
        // return Response, or null if cancelled, or RequestTimeout if timeout occurred.
        public Response BlockingRequest(CancellationToken cancel, Method method, string endpoint, 
                                                string headerData = null, NameValueCollection headers = null,
                                                string contenttype = DefaultContentType, int timeout = DefaultTimeout)
        {
            HttpWebRequest request = MakeRequest(method, endpoint, headerData, headers, contenttype);
            return BlockingRequest(cancel, request, timeout);
        }

        // perform a BlockingRequest but with a cancellation token.
        // Wait for it to complete, or a timeout. Return Response
        // if readbody = true, this function reads the data stream and stores it in rp.Body
        // return Response, or null if cancelled, or RequestTimeout if timeout occurred.
        public Response BlockingRequest(CancellationToken cancel, HttpWebRequest request, int timeout, bool readbody = true)
        {
            ManualResetEvent complete = new ManualResetEvent(false);    // set when request completes
            Response rp = null;     // passed back response

            // do this async in this task, will return.  The callback stores the response and sets the manual reset event to trigger the wait below
            var asyncresult = AsyncRequest((r, o) => { rp = r; complete.Set(); }, null, request, readbody);

            if (asyncresult == null)
                return null;

            var obj = (Tuple<HttpWebRequest, Action<Response, object>, object,bool>)asyncresult.AsyncState;      // get handle to request data

            // Fix hang under mono when performing requests on UI thread
            int waitforresult;

            if (System.Windows.Forms.Application.MessageLoop)
            {
                do
                {
                    int curtimeout = timeout < 0 || timeout > 1000 ? 1000 : timeout;

                    // wait for complete or for cancel, with timeout
                    waitforresult = WaitHandle.WaitAny(new WaitHandle[] { complete, cancel.WaitHandle }, curtimeout);

                    if (waitforresult == WaitHandle.WaitTimeout)
                        System.Windows.Forms.Application.DoEvents();

                    if (timeout > 0)
                        timeout -= curtimeout;
                } while (waitforresult == WaitHandle.WaitTimeout && timeout != 0);
            }
            else
            {
                waitforresult = WaitHandle.WaitAny(new WaitHandle[] { complete, cancel.WaitHandle }, timeout);
            }

            if (waitforresult == WaitHandle.WaitTimeout)
            {
                obj.Item1.Abort();      // grab the request and abort it
                complete.WaitOne();     // will cause the transaction to complete
                return new Response(HttpStatusCode.RequestTimeout);
            }
            else if (waitforresult == 1)        // cancel
            {
                obj.Item1.Abort();      // grab the request and abort it
                complete.WaitOne();     // will cause the transaction to complete
                return null;
            }
            else
            {                                   // complete fired with response
                return rp;
            }
        }

        /// Class holds the status code and headers of the response, and optionally the body in a string
        [System.Diagnostics.DebuggerDisplay("{StatusCode}")]
        public class Response
        {
            public HttpStatusCode StatusCode { get; set; }
            public HttpWebResponse HttpResponse { get; set; }       // set if body is not read
            public string Body { get; set; }                        // filled if body is read
            public NameValueCollection Headers { get; set; }
            public bool Error { get; set; }
            public Response(HttpStatusCode statusCode, string content = null, NameValueCollection headers = null)
            {
                StatusCode = statusCode;
                Body = content;
                Headers = headers;
                Error = (int)statusCode >= 400;
            }

            public static Response Create(HttpWebResponse response, bool readbody)
            {
                if (response == null)
                {
                    WriteLog("HTTPCom Response is null");
                    return new Response(HttpStatusCode.NotFound);
                }

                Response ourresponse;

                if (readbody)
                {
                    var dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    ourresponse = new Response(response.StatusCode, reader.ReadToEnd(), response.Headers);
                    reader.Close();
                    dataStream.Close();
                }
                else
                {
                    ourresponse = new Response(response.StatusCode);
                    ourresponse.HttpResponse= response;
                }

                string d2 = $"HTTPCom Response {response.Method} to {response.ResponseUri.ToString().RemoveApiKey()}: {response.StatusCode}";
                foreach (string hdr in response.Headers.AllKeys)
                {
                    var content = response.Headers[hdr];
                    d2 = d2.AppendPrePad($"  {hdr}:{content}", Environment.NewLine);
                }

                if ( readbody)
                    WriteLog(d2, ourresponse.Body.Left(1024), false);

                return ourresponse;
            }
        }

        // make a request to this endpoint with this method, optional post headerdata.  
        // if endpoint starts with http, its the full url, else its ServerAddress+endpoint
        // content types are defined in https://www.iana.org/assignments/media-types/media-types.xhtml
        // user agent if null uses the UserAgent member, else give specific user agent
        public HttpWebRequest MakeRequest(Method method, string endpoint, string headerData, NameValueCollection headers, string contenttype, string useragent = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? endpoint : ServerAddress + endpoint);
            request.Method = method.ToString();
            request.ContentType = contenttype;
            request.UserAgent = useragent ?? UserAgent;      // set user agent

            string dbgmsg = $"HTTPCom {method} to {ServerAddress + endpoint.RemoveApiKey()} Thread '{System.Threading.Thread.CurrentThread.Name}'";

            if (method == Method.GET)
            {
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            else
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(headerData);  // Set the ContentType property of the WebRequest.
                request.ContentLength = byteArray.Length;       // Set the ContentLength property of the WebRequest.
                Stream dataStream = request.GetRequestStream();     // Get the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);       // Write the data to the request stream.
                dataStream.Close();     // Close the Stream object.
                dbgmsg += " PostData: " + headerData.RemoveApiKey();
            }

            if (headers != null)
                request.Headers.Add(headers);

            foreach (string hdr in request.Headers.AllKeys)
            {
                var content = request.Headers[hdr];
                dbgmsg = dbgmsg.AppendPrePad($"  {hdr}:{content}", Environment.NewLine);
            }

            WriteLog(dbgmsg,null,false);

            return request;
        }

        #endregion

        #region Download file

        // Blocking static download file from URI. Ensure its the http(s):// full path
        // Useragent will be entry assembly
        public static bool DownloadFileFromURI(CancellationToken canceltoken,
                                                string fulluri,                  
                                                string filename,                                
                                                bool alwaysnewfile,                             
                                                out bool downloaded,                               
                                                Action<long, double> reportProgress = null,  
                                                int initialtimeout = 20000)
        {
            HttpCom hp = new HttpCom("");   // no server url
            return hp.DownloadFile(canceltoken, fulluri, filename, alwaysnewfile, out downloaded, reportProgress, initialtimeout);
        }

        // Blocking download file.
        // give endpoint as a full uri (starting with http) or endpoint and the address will be ServerAddress+endpoint
        // store into filename which must be not null and should be a valid location to store into. trapped exception if not and return false.
        // alwaysnewfile if true, etag is not used, always download and return downloaded = true
        //               If false and the etag file is there, and the file is the same, don't download it but return true with downloaded = false. Else download
        // downloaded is set true if the file has been streamed down. 
        // cancelRequested/canceltoken allows cancellation by either means.
        // reportprogress allows feedback on downloads
        // initialtimeout is the timeout between request and response.  Thereafter there is no timeout on the streaming down.
        // does not except.
        // supports GZIP/Deflate
        // return true if file is present already (using etag) or has been downloaded okay. False if an error occurs

        public bool DownloadFile(CancellationToken canceltoken,
                                string uriorendpoint,
                                string filename,
                                bool dontuseetagdownfiles,
                                out bool downloaded,
                                Action<long, double> reportProgress = null,
                                int initialtimeout = 20000)
        {
            downloaded = false;

            System.Diagnostics.Debug.Assert(filename != null);
            System.Diagnostics.Debug.Assert(uriorendpoint != null);

            var etagFilename = dontuseetagdownfiles ? null : filename + ".etag";           // if we give a filename and we are not always asking for a new file, we get a .etag file

            var request = MakeRequest(Method.GET, uriorendpoint, null, null, null);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (etagFilename != null && File.Exists(etagFilename) && File.Exists(filename))   // if we want to etag it, and we have it, and we have the file, we can do a check
            {
                var etag = File.ReadAllText(etagFilename);
                if (etag != "")
                    request.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                else
                    request.IfModifiedSince = File.GetLastWriteTimeUtc(etagFilename);
            }

            WriteLog($"HTTPCom Downloadfile {request.RequestUri} to {filename}");

            Response rp = BlockingRequest(canceltoken, request, initialtimeout, false);         // don't download..

            if (rp == null)     // cancelled
                return false;

            if (rp.StatusCode == HttpStatusCode.NotModified)          // response will be closed as anything that webexcepts closes it.
            {
                return true;
            }

            if (rp.HttpResponse != null)    // if we have a response body to read
            {
                try
                {
                    using (var httpStream = rp.HttpResponse.GetResponseStream())
                    {
                        downloaded = true;
                        var tmpFilename = filename + ".tmp";        // copy thru tmp
                        using (var destFileStream = File.Open(tmpFilename, FileMode.Create, FileAccess.Write))
                        {
                            WriteLog($"HTTPCom Begin download from {request.RequestUri} to {tmpFilename}");

                            reportProgress?.Invoke(0, 0);        // first progress report 0/0

                            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                            long lastreportime = 0;

                            byte[] buffer = new byte[64 * 1024];
                            long count = 0;
                            do
                            {
                                int numread = httpStream.Read(buffer, 0, buffer.Length);        // read in blocks

                                count += numread;

                                var tme = sw.ElapsedMilliseconds;

                                if (numread == 0 || tme - lastreportime >= 1000)       // if at end, or over a second..
                                {
                                    double rate = count / (tme / 1000.0);
                                    reportProgress?.Invoke(count, rate);
                                    WriteLog($"HTTPCom {tme} Downloading {request.RequestUri} {count:N0} at {rate:N2} b/s");
                                    lastreportime = (tme / 1000) * 1000;
                                }

                                if (numread > 0)
                                {
                                    destFileStream.Write(buffer, 0, numread);

                                    if (canceltoken.IsCancellationRequested)
                                    {
                                        destFileStream.Close();
                                        File.Delete(tmpFilename);
                                        rp.HttpResponse.Close();
                                        return false;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                //System.Diagnostics.Debug.WriteLine($"Loaded block {count}");  Thread.Sleep(200);
                            } while (true);
                        }

                        File.Delete(filename);
                        File.Move(tmpFilename, filename);

                        if (etagFilename != null)
                        {
                            File.WriteAllText(etagFilename, rp.HttpResponse.Headers[HttpResponseHeader.ETag]);
                            File.SetLastWriteTimeUtc(etagFilename, rp.HttpResponse.LastModified.ToUniversalTime());
                        }

                        WriteLog($"HTTPCom Finished Download {request.RequestUri} to {filename}");
                        rp.HttpResponse.Close();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"HTTPCom Download exception {request.RequestUri} to {filename} {ex}");
                    rp.HttpResponse.Close();
                }
            }
            else
            {
                //WriteLog($"HTTPCom Downloadfile {request.RequestUri} to {filename} : Status code {rp.StatusCode} {rp.Body}");
            }
            return false;
        }

        // Blocking, download these remote files to localdownloadfolder (or subfolders of it, if Path is set in RemoteFile)
        // you can pass an empty list which still gives true
        // remote files can be http: or endpoints to ServerAddress.  The DownloadURI in remote files gives the location to get it from
        // If remote file has a SHA field, its checked against the SHA of the file, and no download will be performed if its the same
        // or you can use the etag system
        // timeout is per file
        // a local backup folder where you can get a copy of the file can be provided if the download from the internet failed
        // true if all files downloaded okay

        public bool DownloadFiles(CancellationToken cancel,
                                string localdownloadfolderroot,
                                List<RemoteFile> files,
                                bool dontuseetagdownfiles,
                                int perfileinitialtimeout = DefaultTimeout,
                                string localbackupfolder = null)
        {
            localdownloadfolderroot = Path.GetFullPath(localdownloadfolderroot);        // make canonical

            if (!Directory.Exists(localdownloadfolderroot))
                return false;

            foreach (var item in files)
            {
                if (cancel.IsCancellationRequested)
                    return false;

                // if we have a path, make sure folder is there
                if (item.Path.HasChars())
                {
                    string downloadfolder = Path.Combine(localdownloadfolderroot, item.Path);
                    if (!Directory.Exists(downloadfolder))
                        FileHelpers.CreateDirectoryNoError(downloadfolder);
                }

                // synth the local path, which is the root folder, plus its path and name
                string downloadlocalfile = Path.Combine(localdownloadfolderroot, item.Path, item.Name);

                // here we use DownloadNeeded to do a sha comparision if we have an SHA in the remote file descriptor.
                if (item.DownloadNeeded(downloadlocalfile))
                {
                    var downloadok = DownloadFile(cancel, item.DownloadURI, downloadlocalfile, dontuseetagdownfiles, out bool newfile, initialtimeout: perfileinitialtimeout);
                    System.Diagnostics.Debug.WriteLine($"Download {item.FullPath} to {downloadlocalfile} = {downloadok}");

                    if (!downloadok)
                    {
                        // we can use a local backup folder to get files..
                        if ( localbackupfolder != null)
                        {
                            string lb = Path.Combine(localbackupfolder, item.Name);
                            if ( File.Exists(lb))
                            {
                                if (!FileHelpers.TryCopy(lb, downloadlocalfile, true))
                                    return false;
                            }
                        }
                        else
                            return false;
                    }
                }
                else
                {
                    WriteLog($"HTTPCom Download File already present: {item.DownloadURI} -> {downloadlocalfile}");
                }
            }

            return true;
        }

        // Download and optionally synchronise folder
        // see DownloadFiles for parameters

        public bool DownloadFiles(CancellationToken cancel,
                                string localdownloadfolderroot,
                                List<RemoteFile> files,
                                bool dontuseetagdownfiles,
                                bool synchronisefolder,
                                int perfileinitialtimeout = DefaultTimeout,
                                string localbackupfolder = null)
        {
            // if download succeeded

            if (DownloadFiles(cancel, localdownloadfolderroot, files, dontuseetagdownfiles, perfileinitialtimeout, localbackupfolder))
            {
                if (synchronisefolder)
                    return RemoteFile.SynchroniseFolders(localdownloadfolderroot, files);

                return true;
            }

            return false;
        }


        #endregion


        #region Implementation

        // call back handler for AsyncRequest
        private void AysncRespCallback(IAsyncResult asynchronousResult)
        {
            var obj = (Tuple<HttpWebRequest, Action<Response, object>, object,bool>)asynchronousResult.AsyncState;
            HttpWebRequest request = (HttpWebRequest)obj.Item1;

            try
            {
                var response = request.EndGetResponse(asynchronousResult) as HttpWebResponse;

                var responsedata = Response.Create(response,obj.Item4);          // always not null, even if response is null.  Read body is indicated in item4

                if (obj.Item4)            // if read body, we can close
                    response?.Close();

                obj.Item2.Invoke(responsedata, obj.Item3);
            }
            catch( WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                var responsedata = Response.Create(response,true);          // always returns a data object, even if response = null, and download the exception body
                response?.Close();

                obj.Item2.Invoke(responsedata, obj.Item3);
            }
            catch (Exception ex)
            {
                WriteLog("HTTPCom Response Async Exception: " + ex.Message);
                WriteLog($".. stack:", ex.StackTrace);

                obj.Item2.Invoke(new Response(HttpStatusCode.ServiceUnavailable), obj.Item3);
            }
        }

        static protected void WriteLog(string str1, string str2 = null, bool tracelog = true)
        {
            string msg = $"{str1}{(str2 != null ? ": " + str2 : "")}";

            if (tracelog)
                System.Diagnostics.Trace.WriteLine(msg);
            else
                System.Diagnostics.Debug.WriteLine(msg);

            if (LogPath == null || !Directory.Exists(LogPath))
                return;

            try
            {
                lock(writelock)
                {
                    string filename = Path.Combine(LogPath, "HTTP_" + DateTime.Now.ToString("yyyy-MM-dd") + ".hlog");

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        w.WriteLine(DateTime.UtcNow.ToStringZulu() + ": " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("HTTPCom Log Exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }

        private static Object writelock = new Object();
        private string httpserveraddress { get; set; }

        #endregion
    }
}
