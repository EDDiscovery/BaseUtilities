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
    // read first number in the string, return if read..
    
    static public Tuple<double, bool> ReadNumeric(string left, string removetext = null)
    {
        double vleft = 0;
        bool leftgood = false;
        if (left != null)
        {
            if (removetext != null)
                left = left.Replace(removetext, "");

            left = left.Trim();
            int index = left.IndexOfNonNumberDigit(CultureInfo.CurrentCulture);
            if (index >= 0)
                left = left.Substring(0, index);

            if (Double.TryParse(left, out vleft))
                leftgood = true;
        }

        return new Tuple<double, bool>(vleft, leftgood);
    }

    // compare two strings numerically

    static public int CompareNumeric(this string left, string right, string removetext = null)
    {
        var datal = ReadNumeric(left, removetext);
        var datar = ReadNumeric(right, removetext);
        if (datal.Item2 == false)        // left bad, right better
            return 1;
        else if (datar.Item2 == false)   // right bad, left better
            return -1;
        else
            return datal.Item1.CompareTo(datar.Item1);
    }

}


