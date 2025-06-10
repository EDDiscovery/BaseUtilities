/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ObjectExtensionsStrings
{
    //extend for case
    public static bool Contains(this string data, string comparision, StringComparison culture = StringComparison.CurrentCulture)
    {
        return data.IndexOf(comparision, culture) >= 0;
    }

    //Return index of (plus an offset) or length
    public static int IndexOfOrLength(this string data, string comparision, StringComparison culture = StringComparison.CurrentCulture, int startindex = 0, int offset = 0)
    {
        var i = data.IndexOf(comparision, startindex, culture);
        return i == -1 ? data.Length : Math.Min(data.Length, i + offset);
    }
    public static int IndexOfOrLength(this string data, string[] comparision, StringComparison culture = StringComparison.CurrentCulture, int startindex = 0, int offset = 0)
    {
        int min = int.MaxValue;
        foreach (var text in comparision)
        {
            int v = data.IndexOf(text, startindex, culture);
            if (v >= 0 && v < min)
                min = v;
        }

        return min == int.MaxValue ? data.Length : Math.Min(data.Length, min + offset);
    }

    // find the index of the first char matching expression, like Array.FindIndex.
    static public int IndexOf(this string str, Predicate<char> predicate)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (predicate(str[i]))
                return i;
        }
        return -1;
    }

    // find the index of the first char not a number character
    static public int IndexOfNonNumberDigit(this string str, System.Globalization.CultureInfo ci)
    {
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (char.IsDigit(c) || str.Substring(i).StartsWith(ci.NumberFormat.NumberDecimalSeparator) ||
                        str.Substring(i).StartsWith(ci.NumberFormat.NumberGroupSeparator) ||
                        str.Substring(i).StartsWith(ci.NumberFormat.NegativeSign) ||
                        c == 'e' || c == 'E')
            {
            }
            else
                return i;
        }
        return -1;
    }

    // safe index of with culture
    static public int SafeIndexOf(this string str, string find, int start, StringComparison compare = StringComparison.CurrentCulture)
    {
        if (start >= 0 && start < str.Length)
            return str.IndexOf(find, start, compare);
        else
            return -1;
    }


    // if it starts with this, skip it
    public static string Skip(this string s, string t, StringComparison c = StringComparison.InvariantCulture)
    {
        if (s.StartsWith(t, c))
            s = s.Substring(t.Length);
        return s;
    }


    // skip to find first alpha text ignoring whitespace
    public static string FirstAlphaNumericText(this string obj)
    {
        if (obj == null)
            return null;
        else
        {
            string ret = "";
            int i = 0;
            while (i < obj.Length && !char.IsLetterOrDigit(obj[i]))
                i++;

            for (; i < obj.Length; i++)
            {
                if (char.IsLetterOrDigit(obj[i]))
                    ret += obj[i];
                else if (!char.IsWhiteSpace(obj[i]))
                    break;
            }

            return ret;

        }
    }

    public static string RegExWildCardToRegular(this string value)
    {
        if (value.Contains("*") || value.Contains("?"))
        {
            if (value.StartsWith("*"))
            {
                // no anchor start
                return System.Text.RegularExpressions.Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            }
            else
            {
                // anchor start (^), text with ? replaced by . (any) and * by .* (any in a row) and end anchor ($)
                return "^" + System.Text.RegularExpressions.Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            }
        }
        else
        {
            // anchor start, need to escape all chars, anchor end
            return "^" + System.Text.RegularExpressions.Regex.Escape(value) + "$";
        }
    }

    public static bool WildCardMatch(this string value, string match, bool caseinsensitive = false)
    {
        match = match.RegExWildCardToRegular();
        return System.Text.RegularExpressions.Regex.IsMatch(value, match, caseinsensitive ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None);
    }

    // find the next instance of one of the chars in set, in str, and return it in res. Return string after it.  Null if not found 
    static public string NextOneOf(this string str, char[] set, out char res)
    {
        res = char.MinValue;
        int i = str.IndexOfAny(set);
        if (i >= 0)
        {
            res = str[i];
            return str.Substring(i + 1);
        }

        return null;
    }




}

