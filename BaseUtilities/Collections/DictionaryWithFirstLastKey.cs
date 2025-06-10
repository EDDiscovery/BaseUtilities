/*
 * Copyright © 2021 EDDiscovery development team
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
 *
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    // this is a dictionary which tracks the last key added thru ADD

    public class DictionaryWithFirstLastKey<TKey, TValue> : Dictionary<TKey,TValue>
    {
        public TKey FirstKey { get; set; } = default(TKey);     // first one added
        public TKey LastKey { get; set; } = default(TKey);      // last one

        public new void Add(TKey k,TValue v)
        {   
            if ( Count == 0)                                    // if nothing in the dictionary, its first key
                FirstKey = k;
            base.Add(k, v);
            LastKey = k;
        }

        public new TValue this[TKey k] { get { return base[k]; } set { if (Count == 0) FirstKey = k; base[k] = value; LastKey = k; } }
    }
}