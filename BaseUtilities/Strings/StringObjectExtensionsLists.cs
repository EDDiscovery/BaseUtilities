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

public static class ObjectExtensionsStringsLists
{
    // does the list contain comparision
    public static int ContainsIn(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)        //extend for case
    {
        int i = 0;
        foreach (var s in list)
        {
            //System.Diagnostics.Debug.WriteLine("{0} contains {1}", s, comparision);
            if (s.Contains(comparision, c))
            {
                //System.Diagnostics.Debug.WriteLine("..Matched {0} with {1}", s, comparision);
                return i;
            }

            i++;
        }

        return -1;
    }

    // does the comparision contain any in the list
    public static int ComparisionContains(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)        //extend for case
    {
        int i = 0;
        foreach (var s in list)
        {
            //System.Diagnostics.Debug.WriteLine("{0} contains {1}", comparision, s);
            if (comparision.Contains(s, c))
            {
                //System.Diagnostics.Debug.WriteLine("..Matched {0} with {1}", comparision ,s);
                return i;
            }

            i++;
        }

        return -1;
    }

    // does comparision starts with any in list
    public static int StartsWith(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)        //extend for case
    {
        int i = 0;
        foreach (var s in list)
        {
            if (comparision.StartsWith(s, c))
                return i;

            i++;
        }

        return -1;
    }

    // both are separ strings (ssksk;skjsks; etc) is all of contains in semilist.
    public static bool ContainsAllItemsInList(this string semilist, string contains, char separ)
    {
        string[] sl = semilist.SplitNoEmptyStartFinish(separ);
        string[] cl = contains.SplitNoEmptyStartFinish(separ);
        foreach (var s in cl)
        {
            if (Array.IndexOf(sl, s) < 0)
                return false;
        }

        return true;

    }

    public static bool MatchesAllItemsInList(this string semilist, string contains, char separ)
    {
        string[] sl = semilist.SplitNoEmptyStartFinish(separ);
        string[] cl = contains.SplitNoEmptyStartFinish(separ);

        if (sl.Length == cl.Length)
        {
            foreach (var s in cl)
            {
                if (Array.IndexOf(sl, s) < 0)
                    return false;
            }

            return true;
        }
        else
            return false;
    }

    public static string ToStringCommaList(this System.Collections.Generic.List<string> list, int mincount = 100000, bool escapectrl = false, string separ = ", ")
    {
        string r = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i >= mincount && list[i].Length == 0)           // if >= minimum, and zero
            {
                int j = i + 1;
                while (j < list.Count && list[j].Length == 0)   // if all others are zero
                    j++;

                if (j == list.Count)        // if so, stop
                    break;
            }

            if (i > 0)
                r += separ;

            if (escapectrl)
                r += list[i].EscapeControlChars().QuoteString(comma: true);
            else
                r += list[i].QuoteString(comma: true);
        }

        return r;
    }

    public static string ToString(this int[] a, string separ)
    {
        string outstr = "";
        if (a.Length > 0)
        {
            outstr = a[0].ToString(System.Globalization.CultureInfo.InvariantCulture);

            for (int i = 1; i < a.Length; i++)
                outstr += separ + a[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return outstr;
    }

    public static string ToString(this List<int> a, string separ)       // ensure invariant
    {
        string outstr = "";
        if (a.Count > 0)
        {
            outstr = a[0].ToString(System.Globalization.CultureInfo.InvariantCulture);

            for (int i = 1; i < a.Count; i++)
                outstr += separ + a[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return outstr;
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

