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
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

static public class HTTPExtensions
{
    static public string RequestInfo(this HttpListenerRequest rq)
    {
        string s = "Request " + rq.UserHostName + " : " + rq.Url.AbsolutePath;

        if (rq.QueryString.Count > 0)
        {
            for (int i = 0; i < rq.QueryString.Count; i++)
                s = s + Environment.NewLine + "  Key " + rq.QueryString.GetKey(i) + " Value " + rq.QueryString.Get(i);
        }

        return s;
    }

    static public string RequestHeaders(this HttpListenerRequest rq)
    {
        string s = "";
        for (int i = 0; i < rq.Headers.Count; i++)
            s = s.AppendPrePad( rq.Headers.GetKey(i) + " = " + string.Join(";", rq.Headers.GetValues(i)), Environment.NewLine);

        return s;
    }

    // give string s, UTF8 encode it into bytes, gzip the bytes, base64 encode it, then URI Escape it
    public static string URIGZipBase64Escape(this string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);

        using (MemoryStream indata = new MemoryStream(bytes))
        {
            using (MemoryStream outdata = new MemoryStream())
            {
                using (System.IO.Compression.GZipStream gzipStream = new System.IO.Compression.GZipStream(outdata, System.IO.Compression.CompressionLevel.Optimal, true))
                    indata.CopyTo(gzipStream);      // important to clean up gzip otherwise all the data is not written.. using

                var base64 = Convert.ToBase64String(outdata.ToArray());

                return Uri.EscapeDataString(base64);
            }
        }
    }

    // given a string str, URI Escape it
    public static string URIEscapeLongDataString(this string str)
    {
        string ret = "";

        for (int p = 0; p < str.Length; p += 16384)
        {
            ret += Uri.EscapeDataString(str.Substring(p, Math.Min(str.Length - p, 16384)));
        }

        return ret;
    }

    // remove known password strings
    public static string RemoveApiKey(this string str)
    {
        str = Regex.Replace(str, "apiKey=[^&]*", "apiKey=xxx", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "password=[^&]*", "password=xxx", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "\"APIKey\":\".*\"", "\"APIKey\":\"xxx\"", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "\"commanderFrontierID\":\".*\"", "\"commanderFrontierID\":\"xxx\"", RegexOptions.IgnoreCase);
        return str;
    }

    // Make a string of key=value&key2=value pairs
    // Make a string of key=value&key2=value pairs
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
                if (value is string str)
                {
                    sb.AppendPrePad(name + "=" + System.Web.HttpUtility.UrlEncode(str), "&");
                    System.Diagnostics.Debug.WriteLine($"MakeQuery {name} = `{str}`");
                }
                else if (value is string[])
                {
                    foreach (string x in value as string[])
                    {
                        sb.AppendPrePad(name + "=" + System.Web.HttpUtility.UrlEncode(x), "&");
                        System.Diagnostics.Debug.WriteLine($"MakeQuery {name} = `{x}`");
                    }
                }
                else if (value is bool bl)
                {
                    string bs = bl ? "1" : "0";
                    sb.AppendPrePad(name + "=" + bs, "&");
                    System.Diagnostics.Debug.WriteLine($"MakeQuery {name} = `{bs}`");
                }

                else
                {
                    string res = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
                    sb.AppendPrePad(name + "=" + res, "&");
                    System.Diagnostics.Debug.WriteLine($"MakeQuery {name} = `{res}`");
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"MakeQuery is `{sb.ToString()}`");
        return sb.ToString();
    }
}
