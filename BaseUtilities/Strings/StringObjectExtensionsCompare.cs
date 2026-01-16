/*
 * Copyright 2021 - 2025 EDDiscovery development team
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
using System.Globalization;

public static class ObjectExtensionsStringsCompare
{
    // read first number in the string, return if read..
    
    static public Tuple<double, bool> ReadNumeric(string left, string removetext = null, bool striptonumeric = false)
    {
        double vleft = 0;
        bool leftgood = false;
        if (left != null)
        {
            if (striptonumeric)
            {
                while (left.Length > 0 && !left[0].IsDigitOrDotOrNegSign())
                    left = left.Substring(1);
            }
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

    static public int CompareNumeric(this string left, string right, string removetext = null, bool striptonumeric = false)
    {
        var datal = ReadNumeric(left, removetext, striptonumeric);
        var datar = ReadNumeric(right, removetext, striptonumeric);
        
        if (datal.Item2 == false)        // left bad
            return datar.Item2 == false ? 0 : 1;          // if both bad, same, else less
        else if (datar.Item2 == false)   // right bad, left better
            return -1;
        else
        {
            var res = datal.Item1.CompareTo(datar.Item1);
            return res;
        }
    }

    static public int CompareAlphaInt(this string x, string y)
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
                    int xv = xs.InvariantParseInt(0);
                    int yv = ys.InvariantParseInt(0);
                    int v = xv.CompareTo(yv);
               //     System.Diagnostics.Debug.WriteLine($"CAIn {xv} vs {yv} = {v}");
                    if (v != 0)     // if we have a result, stop
                        return v;

                    i = xp-1;     // they must be the same numbers set i to one less end char due to i++ above
                }
                else
                    return -1;
            }
            else
            {
                int v = x[i].CompareTo(y[i]);       // alpha compare
            //    System.Diagnostics.Debug.WriteLine($"CAIa {x[i]} vs {y[i]} = {v}");
                if (v != 0)
                    return v;
            }
        }

        int ev = x.Length < y.Length ? -1 : 0;        // if we stopped because x is shorter than y, then x is smaller. Else same length, all equal
      //  System.Diagnostics.Debug.WriteLine($"CAIend {x.Length} {y.Length} = {ev}");
        return ev;
    }

    // these accept s = null without barfing

    public static bool EqualsIIC(this string s, string other)
    {
        return s != null && s.Equals(other, StringComparison.InvariantCultureIgnoreCase);
    }
    public static bool StartsWithIIC(this string s, string other)
    {
        return s!= null && s.StartsWith(other, StringComparison.InvariantCultureIgnoreCase);
    }
    public static bool EndsWithIIC(this string s, string other)
    {
        return s != null && s.EndsWith(other, StringComparison.InvariantCultureIgnoreCase);
    }

}


