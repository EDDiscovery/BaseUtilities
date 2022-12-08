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
 */

using System;
using System.Collections.Generic;
using System.Linq;

public static partial class ObjectExtensionsStrings
{
    // obj = null, return "".  Length can be > string
    public static string Left(this string obj, int length)
    {
        if (obj != null)
        {
            if (length < obj.Length)
                return obj.Substring(0, length);
            else
                return obj;
        }
        else
            return string.Empty;
    }

    // obj = null/empty, return "". If text in string, return stuff left of it. If text not in string, return either empty or all of it
    public static string LeftOf(this string obj, string match, StringComparison cmp = StringComparison.CurrentCulture, bool allifnotthere = false)
    {
        if (obj != null && obj.Length > 0)
        {
            int indexof = obj.IndexOf(match, cmp);
            if (indexof == -1)
                return allifnotthere ? obj : "";
            else
                return obj.Substring(0, indexof);
        }
        else
            return string.Empty;
    }

    // obj = null, return "". start/length can be out of limits
    public static string Mid(this string obj, int start, int length = 999999)
    {
        if (obj != null)
        {
            if (start < obj.Length)        // if in range
            {
                int left = obj.Length - start;      // what is left..
                return obj.Substring(start, Math.Min(left, length));    // min of left, length
            }
        }

        return string.Empty;
    }

    // obj = null/empty, return "". If text in string, return stuff from it onwards. If text not in string, return either empty or all of it
    public static string MidOf(this string obj, string match, StringComparison cmp = StringComparison.CurrentCulture, bool allifnotthere = false)
    {
        if (obj != null && obj.Length > 0)
        {
            int indexof = obj.IndexOf(match, cmp);
            if (indexof == -1)
                return allifnotthere ? obj : "";
            else
                return obj.Substring(indexof);
        }
        else
            return string.Empty;
    }

    public static string Truncate(this string str, int start, int length, string endmarker = "")
    {
        if (str == null)                // nothing, return empty
            return "";

        int len = str.Length - start;
        if (len < 1)                    // if start beyond length
            return "";
        else if (len > length)           // if we need to cut, because len left > length allowed
            return str.Substring(start, length) + endmarker;
        else
            return str.Substring(start);        // len left is less than length, return the whole lot
    }

    static public string StripDigits(this string s, ref int i)
    {
        int start = i;
        while (i < s.Length && char.IsDigit(s[i]))
            i++;

        return s.Substring(start, i - start);
    }

    // if prefix, slice it off
    public static bool IsPrefixRemove(ref string s, string t, StringComparison c = StringComparison.InvariantCulture)
    {
        if (s.StartsWith(t, c))
        {
            s = s.Substring(t.Length);
            return true;
        }
        return false;
    }

    public static string RemoveTrailingCZeros(this string str)
    {
        int index = str.IndexOf('\0');
        if (index >= 0)
            str = str.Substring(0, index);
        return str;
    }


    // mimics Split('s') if emptyendifmarkersatend is true
    static public string[] Split(this string s, string splitchars, StringComparison cmp = StringComparison.InvariantCultureIgnoreCase,
                                    bool emptyendifmarkeratend = false, bool emptyarrayifempty = false)
    {
        if (s == null)
            return null;

        var start = new int[s.Length];
        var len = new int[s.Length];
        var sections = 0;

        int ipos = 0;
        for (ipos = 0; ipos < s.Length;)            // ipos is left at start of last section, or may be at s.Length
        {
            int nextpos = s.IndexOf(splitchars, ipos, cmp);
            if (nextpos >= 0)
            {
                start[sections] = ipos;
                len[sections++] = nextpos - ipos;
                ipos = nextpos + splitchars.Length;

                if (ipos == s.Length && emptyendifmarkeratend)  // marker at end.. and we want the normal split behaviour
                {
                    start[sections] = 0;
                    len[sections++] = 0;
                }
            }
            else
            {
                start[sections] = ipos;             // if not found, add last section
                len[sections++] = s.Length - ipos;
                break;
            }
        }

        if (sections == 0)
        {
            return emptyarrayifempty ? new string[] { } : new string[] { "" };     // mimic "".split('') behaviour or empty array
        }
        else
        {
            string[] ret = new string[sections];
            for (int j = 0; j < sections; j++)
                ret[j] = s.Substring(start[j], len[j]);
            return ret;
        }
    }

    static public string[] SplitNoEmptyStartFinish(this string s, char splitchar)
    {
        string[] array = s.Split(splitchar);
        int start = array[0].Length > 0 ? 0 : 1;
        int end = array.Last().Length > 0 ? array.Length - 1 : array.Length - 2;
        int length = end - start + 1;
        return length == array.Length ? array : array.RangeSubset(start, length);
    }

    static public List<string> SplitNoEmptyStrings(this string s, char splitchar)
    {
        string[] array = s.Split(splitchar);
        List<string> entries = new List<string>();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Length > 0)
                entries.Add(array[i]);
        }

        return entries;
    }



}

