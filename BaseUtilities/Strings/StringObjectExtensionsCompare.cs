/*
 * Copyright © 2021 - 2021 EDDiscovery development team
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
using System.Globalization;

public static class ObjectExtensionsStringsCompare
{
    static public int CompareNumeric(this string left, string right, string removetext = null)
    {
        double vleft = 0;
        if (left != null)
        {
            if (removetext != null)
                left = left.Replace(removetext, "");

            left = left.Trim();
            int index = left.IndexOfNonNumberDigit(CultureInfo.CurrentCulture);
            if (index >= 0)
                left = left.Substring(0, index);

            if (!Double.TryParse(left, out vleft))
                return 1;
        }
        else
            return 1;

        // s1 is not null, and v1 is set

        double vright = 0;
        if (right != null)
        {
            if (removetext != null)
                right = right.Replace(removetext, "");

            right = right.Trim();
            int index = right.IndexOfNonNumberDigit(CultureInfo.CurrentCulture);
            if (index >= 0)
                right = right.Substring(0, index);

            if (!Double.TryParse(right, out vright))
                return -1;
        }
        else
            return -1;

        return vleft.CompareTo(vright);
    }

}


