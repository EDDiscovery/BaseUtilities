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
    // in array, find first occurance of any of the array[n] terms in s, return -1 not found, or set arrayindex to the one found and return the position in s where found
    static public int IndexOf(this string s, string[] array, out int arrayindex)   
    {
        int found = -1;
        arrayindex = -1;
        for (int av = 0; av < array.Length; av++)
        {
            int pos = s.IndexOf(array[av]);
            if (pos != -1 && (found == -1 || pos < found))
            {
                found = pos;
                arrayindex = av;
            }
        }
        return found;
    }

    // Case insensitive version of Array.Indexof
    static public int IndexOf(this string[] array, string text, StringComparison compare ) 
    {
        for (int av = 0; av < array.Length; av++)
        {
            if (array[av].HasChars() && array[av].Equals(text, compare))
                return av;
        }

        return -1;
    }
}

