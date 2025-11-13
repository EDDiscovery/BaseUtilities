/*
 * Copyright 2016 - 2025 EDDiscovery development team
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
using System.Text;

public static partial class ObjectExtensionsStrings
{
    static public bool HasChars(this string obj)
    {
        return obj != null && obj.Length > 0;
    }
    static public bool HasNonSpaceChars(this string obj)
    {
        if (obj != null && obj.Length > 0)
        {
            foreach( var c in obj)
            {
                if (c != ' ')
                    return true;
            }
        }
        return false;
    }
    static public bool HasLetterChars(this string obj)
    {
        if (obj != null && obj.Length > 0)
        {
            foreach (char x in obj)
            {
                if (char.IsLetter(x))
                    return true;
            }
        }
        return false;
    }

    // is this string full of the predicate
    static public bool HasAll(this string obj, Predicate<char> test)
    {
        if (obj != null && obj.Length > 0)
        {
            foreach (char x in obj)
            {
                if (!test(x))
                    return false;
            }

            return true;
        }
        return false;
    }

    static public bool IsEmpty(this string obj)
    {
        return obj == null || obj.Length == 0;
    }

    public static bool IsLetterOrUnderscore(this char v)
    {
        return char.IsLetter(v) || v == '_';
    }
    public static bool IsLetterOrDigitOrUnderscore(this char v)
    {
        return char.IsLetterOrDigit(v) || v == '_';
    }
    public static bool IsDigitOrDotOrNegSign(this char v)
    {
        return char.IsDigit(v) || v == '.' || v == '-';
    }

    public static bool IsLetterOrDigitOrUnderscoreOrMember(this char v)
    {
        return char.IsLetterOrDigit(v) || v == '_' || v == '.';
    }

    public static bool IsVariable(this string varname, bool member = false)
    {
        if (varname.HasChars() && varname[0].IsLetterOrUnderscore())
        {
            for (int i = 1; i < varname.Length; i++)
            {
                if (member ? !varname[i].IsLetterOrDigitOrUnderscoreOrMember() : !varname[i].IsLetterOrDigitOrUnderscore())
                    return false;
            }

            return true;
        }

        return false;
    }

    public static bool EqualsAlphaNumOnlyNoCase(this string left, string right)
    {
        left = left.Replace("_", "").Replace(" ", "").ToLowerInvariant();        // remove _, spaces and lower
        right = right.Replace("_", "").Replace(" ", "").ToLowerInvariant();
        return left.Equals(right);
    }

    public static int ApproxMatch(this string str, string other, int min)       // how many runs match between the two strings
    {
        int total = 0;
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; i < str.Length && j < other.Length; j++)
            {
                if (str[i] == other[j])
                {
                    int i2 = i + 1, j2 = j + 1;

                    int count = 1;
                    while (i2 < str.Length && j2 < other.Length && str[i2] == other[j2])
                    {
                        count++;
                        i2++;
                        j2++;
                    }

                    //if ( count>1)  System.Diagnostics.Debug.WriteLine("Match " + str.Substring(i) + " vs " + other.Substring(j) + " " + count);
                    if (count >= min)   // at least this number of chars in a row.
                    {
                        total += count;
                        i += count;
                        //System.Diagnostics.Debug.WriteLine(" left " + str.Substring(i));
                    }
                }
            }
        }

        //System.Diagnostics.Debug.WriteLine("** TOTAL " + str + " vs " + other + " " + total);

        return total;
    }


}

