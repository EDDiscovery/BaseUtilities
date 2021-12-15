/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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

public static class ObjectExtensionsStringsPick
{
    // pick one of x;y;z or if ;x;y;z, pick x and one of y or z
    // separs are case sensitive

    public static string PickOneOf(this string str, string separ, System.Random rx)    
    {
        string[] a = str.Split(separ, StringComparison.InvariantCulture, emptyendifmarkeratend:true);

        if (a.Length >= 2)          // x;y
        {
            if (a[0].Length == 0)      // ;y      
            {
                string res = a[1];
                if (a.Length > 2)   // ;y;x;z
                    res += " " + a[2 + rx.Next(a.Length - 2)];

                return res;
            }
            else
                return a[rx.Next(a.Length)];
        }
        else
            return a[0];
    }

    // pick one of x;y;z or if ;x;y;z, pick x and one of y or z, include {x;y;z}

    public static string PickOneOfGroups(this string exp, System.Random rx, string separ = ";", string groupstart = "{", string groupend = "}") 
    {
        string res = "";
        exp = exp.Trim();

        while (exp.Length > 0)
        {
            if (exp.StartsWith(groupstart))
            {
                int end = exp.IndexOf(groupend);

                if (end == -1)              // missing end bit, assume the lot..
                    end = exp.Length;

                string pl = exp.Substring(groupstart.Length, end - groupstart.Length);

                exp = (end + groupend.Length < exp.Length) ? exp.Substring(end + groupend.Length) : "";

                res += pl.PickOneOf(separ, rx);
            }
            else
            {
                int nextgroup = exp.IndexOf(groupstart);

                if (nextgroup >= 0)
                {
                    string pl = exp.Substring(0, nextgroup);
                    exp = exp.Substring(nextgroup);

                    res += pl.PickOneOf(separ, rx);
                }
                else
                {
                    res += exp.PickOneOf(separ, rx);          // thats all left, no more groups, pick
                    break;
                }
            }
        }

        return res;
    }



    //string t = "{{group1<!>group2}} and {{group3<!>group4}}";
    //string r = t.PickOneOfGroups(new Random(), "<!>", "{{", "}}");

}

