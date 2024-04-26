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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ObjectExtensionsStrings
{
    // is string s in any of the array elements
    // return index
    static public int ContainsIn(this string[] array, string s, StringComparison compare = StringComparison.CurrentCulture)
    {
        for (int av = 0; av < array.Length; av++)
        {
            if (array[av].Contains(s, compare))
                return av;
        }

        return -1;
    }

    // does any array element contain s with case control (missing from c#)  
    // return index.
    static public int Contains(this string[] array, string s, StringComparison compare)
    {
        for (int av = 0; av < array.Length; av++)
        {
            if (s.Contains(array[av], compare))
                return av;
        }

        return -1;
    }

    // Does the text match one of the array elements, if so, return number of element (>0) or -1 if not
    static public int Equals(this string[] array, string text, StringComparison compare = StringComparison.CurrentCulture)
    {
        for (int av = 0; av < array.Length; av++)
        {
            if (array[av].HasChars() && array[av].Equals(text, compare))
                return av;
        }

        return -1;
    }

    // in array, find first occurance of any of the array[n] terms in s, return -1 not found, or set arrayindex to the one found and return the position in s where found
    static public int IndexOf(this string s, string[] array, out int arrayindex, StringComparison culture = StringComparison.CurrentCulture, int startindex = 0)   
    {
        int found = -1;
        arrayindex = -1;
        for (int av = 0; av < array.Length; av++)
        {
            int pos = s.IndexOf(array[av],startindex,culture);
            if (pos != -1 && (found == -1 || pos < found))
            {
                found = pos;
                arrayindex = av;
            }
        }
        return found;
    }

    static public string Join(this string[] array, char text)
    {
        return string.Join(new string(new char[] { text }), array);
    }
    static public string Join(this string[] array, string text)
    {
        return string.Join(text, array);
    }

}

