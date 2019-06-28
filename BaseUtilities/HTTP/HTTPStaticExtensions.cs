/*
 * Copyright © 2016 EDDiscovery development team
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
using System.Net;

static public class HTTPStaticExtensions
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

}



