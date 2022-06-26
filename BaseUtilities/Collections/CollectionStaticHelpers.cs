﻿/*
 * Copyright © 2015 - 2022 EDDiscovery development team
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

public static class CollectionStaticHelpers
{
    static public bool Equals<T>(T[] left, T[] right) where T : System.IEquatable<T>        // equals, calling null/null true..
    {
        if (left == null && right == null)
            return true;

        if (left == null || right == null || left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            if (!left[i].Equals(right[i]))
                return false;
        }

        return true;
    }

    static public bool Equals<T>(List<T> left, List<T> right) where T : System.IEquatable<T>
    {
        if (left == null && right == null)
            return true;

        if (left == null || right == null || left.Count != right.Count)
            return false;

        for (int i = 0; i < left.Count; i++)
        {
            if (!left[i].Equals(right[i]))
                return false;
        }

        return true;
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source) => source != null ? source : System.Linq.Enumerable.Empty<T>();

    public class BasicLengthBasedNumberComparitor<TKey> : IComparer<string> where TKey : IComparable   
    {
        public int Compare(string x, string y)
        {
            if (x.Length > 0 && Char.IsDigit(x[0]))      // numbers..
            {
                if (x.Length < y.Length)                // if different length numbers, select, else the ascii comparitor will work
                    return -1;
                else if (x.Length > y.Length)
                    return 1;

            }

            return StringComparer.InvariantCultureIgnoreCase.Compare(x, y);
        }
    }

    public class IntNumberComparitorAllowingExtraText<TKey> : IComparer<string> where TKey : IComparable
    {
        public int Compare(string x, string y)
        {
            int? xv = x.InvariantParseIntNullIgnoreTextAfter();
            int? yv = y.InvariantParseIntNullIgnoreTextAfter();

            if (xv.HasValue && yv.HasValue)
            {
                int r = xv.Value.CompareTo(yv.Value);
                if (r != 0)  // if compare is different, return it, else just use ASCII compare
                    return r;
            }

            return StringComparer.InvariantCultureIgnoreCase.Compare(x, y);
        }
    }

    public class AlphaIntCompare<TKey> : IComparer<string> where TKey : IComparable
    {
        public int Compare(string x, string y)
        {
            return ObjectExtensionsStringsCompare.CompareAlphaInt(x, y);
        }
    }

}

