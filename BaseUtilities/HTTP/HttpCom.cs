/*
 * Copyright © 2016-2023 EDDiscovery development team
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseUtils
{
    public class HttpCom
    {
        static public string LogPath { get; set; } = null;           // set path to cause logging to occur

        // content types are defined in https://www.iana.org/assignments/media-types/media-types.xhtml
        // json is the most widely deployed so is the default
        protected ResponseData RequestPost(string postData, string action, NameValueCollection headers = null, bool handleException = false, string contenttype = "application/json; charset=utf-8")
        {
            return Request("POST", postData, action, headers, handleException,0, contenttype);
        }

        protected ResponseData RequestPatch(string postData, string action, NameValueCollection headers = null, bool handleException = false, string contenttype = "application/json; charset=utf-8")
        {
            return Request("PATCH", postData, action, headers, handleException,0, contenttype);
        }

        protected ResponseData RequestGet(string action, NameValueCollection headers = null, bool handleException = false, int timeout = 5000, string contenttype = "application/json; charset=utf-8")
        {
            return Request("GET", "", action, headers, handleException, timeout, contenttype);
        }

        // responsecallback is in TASK you must convert back to foreground
        protected void RequestGetAsync(string action, Action<ResponseData,Object> responsecallback, Object tag = null, 
                                        NameValueCollection headers = null, bool handleException = false, int timeout = 5000, string contenttype = "application/json; charset=utf-8")
        {
            Task.Factory.StartNew(() =>
            {
                ResponseData resp = Request("GET", "", action, headers, handleException, timeout, contenttype);
                responsecallback(resp,tag);
            });
        }

        // method = "POST", "GET", "PATCH"
        // postdata only for post, normally json. Otherwise empty or null.
        // endpoint = end point to access, added to httpserveraddress in class ("journal")
        // headers = HTTP headers to send, may be null
        // handleException = true, swallow exceptions. Else this will throw
        // timeout = timeout!
        // content type must be set
        // return ResponseData Always.  Status code is Unauthorized (no HTTP path), BadRequest (exception due to data), or server response.

        private ResponseData Request(string method, string postData, string endpoint, NameValueCollection headers, bool handleException,
                                        int timeout, string contenttype)
        {
            if (httpserveraddress == null || httpserveraddress.Length == 0)           // for debugging, set _serveraddress empty
            {
                System.Diagnostics.Trace.WriteLine(method + RemoveApiKey(endpoint));
                return new ResponseData(HttpStatusCode.Unauthorized);
            }

            try
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(httpserveraddress + endpoint);
                    request.Method = method;
                    request.ContentType = contenttype;

                    string dbgmsg = $"HTTP {method} to {httpserveraddress + RemoveApiKey(endpoint)} Thread '{System.Threading.Thread.CurrentThread.Name}'";

                    if (method == "GET")
                    {
                        request.Headers.Add("Accept-Encoding", "gzip,deflate");
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        request.Timeout = timeout;
                    }
                    else
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);  // Set the ContentType property of the WebRequest.
                        request.ContentLength = byteArray.Length;       // Set the ContentLength property of the WebRequest.
                        Stream dataStream = request.GetRequestStream();     // Get the request stream.
                        dataStream.Write(byteArray, 0, byteArray.Length);       // Write the data to the request stream.
                        dataStream.Close();     // Close the Stream object.
                        dbgmsg += " PostData: " + RemoveApiKey(postData);
                    }

                    if (headers != null)
                        request.Headers.Add(headers);

                    foreach (string hdr in request.Headers.AllKeys)
                    {
                        var content = request.Headers[hdr];
                        dbgmsg = dbgmsg.AppendPrePad($"  {hdr}:{content}", Environment.NewLine);
                    }

                    System.Diagnostics.Trace.WriteLine(dbgmsg);
                    WriteLog(dbgmsg,"");

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    var data = getResponseData(response);

                    string d2 = $"HTTP {method} to {httpserveraddress + RemoveApiKey(endpoint)} Response {data.StatusCode}";
                    foreach (string hdr in response.Headers.AllKeys)
                    {
                        var content = response.Headers[hdr];
                        d2 = d2.AppendPrePad($"  {hdr}:{content}", Environment.NewLine);
                    }

                    System.Diagnostics.Trace.WriteLine(d2);
                    WriteLog(d2, data.Body.Left(1024));

                    response.Close();


                    return data;
                }
                catch (WebException ex)
                {
                    if (!handleException)
                    {
                        throw;
                    }

                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        var data = getResponseData(httpResponse);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                        System.Diagnostics.Trace.WriteLine("WebException : " + ex.Message);
                        if (httpResponse != null)
                        {
                            System.Diagnostics.Trace.WriteLine("Response code : " + httpResponse.StatusCode);
                            System.Diagnostics.Trace.WriteLine("Response body : " + data.Body);
                        }

                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        if (LogPath != null)
                        {
                            WriteLog("WebException" + ex.Message, "");
                            if (httpResponse != null)
                            {
                                WriteLog($"HTTP Error code: {httpResponse.StatusCode}", "");
                                WriteLog($"HTTP Error body: {data.Body}", "");
                            }
                        }
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!handleException)
                {
                    throw;
                }

                System.Diagnostics.Trace.WriteLine("Exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                WriteLog("Exception" + ex.Message, "");
 
                return new ResponseData(HttpStatusCode.BadRequest);
            }
        }

        protected string RemoveApiKey(string str)
        {
            str = Regex.Replace(str, "apiKey=[^&]*", "apiKey=xxx", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "password=[^&]*", "password=xxx", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "\"APIKey\":\".*\"", "\"APIKey\":\"xxx\"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "\"commanderFrontierID\":\".*\"", "\"commanderFrontierID\":\"xxx\"", RegexOptions.IgnoreCase);
            return str;
        }

        protected static string EscapeLongDataString(string str)
        {
            string ret = "";

            for (int p = 0; p < str.Length; p += 16384)
            {
                ret += Uri.EscapeDataString(str.Substring(p, Math.Min(str.Length - p, 16384)));
            }

            return ret;
        }

        private static Object LOCK = new Object();
        static public  void WriteLog(string str1, string str2)
        {
            //System.Diagnostics.Debug.WriteLine("From:" + Environment.StackTrace.StackTrace("WriteLog",10) + Environment.NewLine + "HTTP:" + str1 + ":" + str2);

            if (LogPath == null || !Directory.Exists(LogPath))
                return;

            if (str1 != null && str1.ToLowerInvariant().Contains("password"))
                str1 = "** This string contains a password so not logging it.**";

            if (str2 != null && str2.ToLowerInvariant().Contains("password"))
                str2 = "** This string contains a password so not logging it.**";

            try
            {
                lock(LOCK)
                {
                    string filename = Path.Combine(LogPath, "HTTP_" + DateTime.Now.ToString("yyyy-MM-dd") + ".hlog");

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        w.WriteLine(DateTime.UtcNow.ToStringZulu() + ": " + str1 + ": " + str2);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }

        private ResponseData getResponseData(HttpWebResponse response, bool? error = null)
        {
            if (response == null)
                return new ResponseData(HttpStatusCode.NotFound);

            var dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            var data = new ResponseData(response.StatusCode, reader.ReadToEnd(), response.Headers);
            reader.Close();
            dataStream.Close();
            return data;
        }

        public static string MakeQuery(params System.Object[] values)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < values.Length;)
            {
                string name = values[i] as string;
                object value = values[i + 1];
                i += 2;
                if (value != null)
                {
                    if (sb.Length > 0)
                        sb.Append('&');
                    if (value is string)
                        sb.Append(name + "=" + System.Web.HttpUtility.UrlEncode(value as string));
                    else if (value is bool)
                        sb.Append(name + "=" + (((bool)value) ? "1" : "0"));
                    else
                        sb.Append(name + "=" + Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }


        protected string httpserveraddress { get; set; }
    }
}
