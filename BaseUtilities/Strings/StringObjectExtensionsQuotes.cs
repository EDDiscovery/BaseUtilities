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

public static class ObjectExtensionsStringsQuotes
{
    public static string QuoteString(this string obj, bool comma = false, bool bracket = false, bool space = true)
    {
        if (obj.Length == 0 || obj.Contains("\"") || obj[obj.Length - 1] == ' ' || (space && obj.Contains(" ")) || (bracket && obj.Contains(")")) || (comma && obj.Contains(",")))
            obj = "\"" + obj.Replace("\"", "\\\"") + "\"";

        return obj;
    }

    public static string QuoteStringSel(this string obj, char quote = '"' , bool comma = false, bool bracket = false, bool space = true)
    {
        if (obj.Length == 0 || obj.Contains(quote) || obj[obj.Length - 1] == ' ' || (space && obj.Contains(" ")) || (bracket && obj.Contains(")")) || (comma && obj.Contains(",")))
            obj = quote + obj.Replace(quote.ToString(), "\\" + quote.ToString()) + quote;

        return obj;
    }

    public static string AlwaysQuoteString(this string obj)
    {
        return "\"" + obj.Replace("\"", "\\\"") + "\"";
    }

    public static string QuoteStrings(this string[] obja)
    {
        string res = "";
        foreach (string obj in obja)
        {
            if (res.Length > 0)
                res += ",";

            res += "\"" + obj.Replace("\"", "\\\"") + "\"";
        }

        return res;
    }

    public static bool InQuotes(this string s, int max)            // left true if quote left over on line, taking care of any escapes..
    {
        bool inquote = false;
        char quotechar = ' ';

        for (int i = 0; i < max; i++)
        {
            if (s[i] == '\\' && i < max - 1 && s[i + 1] == quotechar)
                i += 1;     // ignore this, ignore "
            else if (s[i] == '"' || s[i] == '\'')
            {
                quotechar = s[i];
                inquote = !inquote;
            }
        }

        return inquote;
    }


}

