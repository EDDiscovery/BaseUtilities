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

public static class ObjectExtensionsIEnumerable
{
    // does the list contain comparision
    public static int ContainsIn(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture, bool ignoreempty = false)
    {
        int i = 0;
        foreach (var s in list)
        {
            //System.Diagnostics.Debug.WriteLine("{0} contains {1}", s, comparision);
            if ((s.Length > 0 || !ignoreempty) && s.Contains(comparision, c))
            {
                //System.Diagnostics.Debug.WriteLine("..Matched {0} with {1}", s, comparision);
                return i;
            }

            i++;
        }

        return -1;
    }

    // does the comparision contain any in the list
    public static int ComparisionContains(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture, bool ignoreempty = false)
    {
        int i = 0;
        foreach (var s in list)
        {
            //System.Diagnostics.Debug.WriteLine("{0} contains {1}", comparision, s);
            if ((s.Length > 0 || !ignoreempty) && comparision.Contains(s, c))
            {
                //System.Diagnostics.Debug.WriteLine("..Matched {0} with {1}", comparision ,s);
                return i;
            }

            i++;
        }

        return -1;
    }

    // does a list element equals comparision
    public static int Equals(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture, bool ignoreempty = false)
    {
        int i = 0;
        foreach (var s in list)
        {
            //System.Diagnostics.Debug.WriteLine("{0} contains {1}", s, comparision);
            if ((s.Length > 0 || !ignoreempty) && s.Equals(comparision, c))
            {
                //System.Diagnostics.Debug.WriteLine("..Matched {0} with {1}", s, comparision);
                return i;
            }

            i++;
        }

        return -1;
    }

    // does comparision starts with any in list
    public static int StartsWith(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture, bool ignoreempty = false)
    {
        int i = 0;
        foreach (var s in list)
        {
            if ((s.Length > 0 || !ignoreempty) && comparision.StartsWith(s, c))
                return i;

            i++;
        }

        return -1;
    }

    // count of elements contained within str with case control (missing from c#)
    // return count.
    static public int ContainsCount(this string str, IEnumerable<string> list, StringComparison compare)
    {
        int count = 0;
        foreach (var s in list)
        {
            if (str.Contains(s, compare))
                count++;
        }

        return count;
    }

    // return index of comparision found in list
    public static int StartsWithInList(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)
    {
        int i = 0;
        foreach (var s in list)
        {
            if (s.StartsWith(comparision, c))
                return i;

            i++;
        }

        return -1;
    }

    // return index of comparision found in list
    public static int EndsWithInList(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)
    {
        int i = 0;
        foreach (var s in list)
        {
            if (s.EndsWith(comparision, c))
                return i;

            i++;
        }

        return -1;
    }


    // in array, find first occurance of any of the array[n] terms in s, return -1 not found, or set arrayindex to the one found and return the position in s where found
    static public int IndexOf(this string s, IEnumerable<string> list, out int arrayindex, StringComparison culture = StringComparison.CurrentCulture, int startindex = 0)
    {
        int found = -1;
        arrayindex = -1;
        int index = 0;
        foreach (var element in list)
        {
            int pos = s.IndexOf(element, startindex, culture);
            if (pos != -1 && (found == -1 || pos < found))
            {
                found = pos;
                arrayindex = index;
            }
            index++;
        }
        return found;
    }

    static public int IndexOf<T>(this IEnumerable<T> list, T text)
    {
        int index = 0;
        foreach(var element in list)
        {
            if (element.Equals(text))
                return index;
            index++;
        }

        return -1;
    }

}

