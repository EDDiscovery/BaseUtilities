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
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    // a sorted list of doubles, allowing duplicate entries

    public class SortedListDoubleDuplicate<TK> : SortedList<double, TK>
    {
        public SortedListDoubleDuplicate() : base(new DuplicateKeyComparer<double>())
        {
        }

        private class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable      // special compare for sortedlist
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);
                return (result == 0) ? 1 : result;      // for this, equals just means greater than, to allow duplicate distance values to be added.
            }
        }
    }

    // more generic version after a few more years of doing this stuff!
    public class SortedListDuplicate<TV,TK> : SortedList<TV, TK> where TV:IComparable
    {
        public SortedListDuplicate() : base(new DuplicateKeyComparer<TV>())
        {
        }

        private class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable      // special compare for sortedlist
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);
                return (result == 0) ? 1 : result;      // for this, equals just means greater than, to allow duplicate distance values to be added.
            }
        }
    }
}
