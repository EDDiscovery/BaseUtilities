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

    static public int CompareAlphaInt(string x, string y)
    {
        for (int i = 0; i < x.Length; i++)
        {
            if (i >= y.Length)      // if beyond, x is greater
                return 1;
            else if (char.IsDigit(x[i]) && char.IsDigit(y[i]))      // if both digits
            {
                int xp = i;
                string xs = x.StripDigits(ref xp);
                int yp = i;
                string ys = y.StripDigits(ref yp);
                if (xp != i && yp != i)     // if got digits
                {
                    int v = xs.InvariantParseInt(0).CompareTo(ys.InvariantParseInt(0));
                    if (v != 0)
                        return v;
                    i = xp;     // they must be the same numbers.
                }
                else
                    return -1;
            }
            else
            {
                int v = x[i].CompareTo(y[i]);       // alpha compare
                if (v != 0)
                    return v;
            }
        }

        return 0;
    }


}


