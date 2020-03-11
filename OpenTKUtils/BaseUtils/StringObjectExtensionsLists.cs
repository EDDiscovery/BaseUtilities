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
    // does comparision exists in list
    public static int ContainsIn(this IEnumerable<string> list, string comparision, StringComparison c = StringComparison.CurrentCulture)        //extend for case
    {
        int i = 0;
        foreach (var s in list)
        {
            if (s.Contains(comparision, c))
                return i;

            i++;
        }

        return -1;
    }

    public static int ContainsIn(this string refs, IEnumerable<string> list, StringComparison c = StringComparison.CurrentCulture)        //extend for case
    {
        int i = 0;
        foreach (var s in list)
        {
            if (refs.Contains(s, c))
                return i;

            i++;
        }

        return -1;
    }

}

