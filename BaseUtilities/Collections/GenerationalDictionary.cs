﻿/*
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    // this is a dictionary which holds a history, by generation, of previous values of the key

    public class GenerationalDictionary<TKey, TValue>
    {
        public uint Generation { get; private set; } = 0;

        private Dictionary<TKey, List<Tuple<uint, TValue>>> dictionary = new Dictionary<TKey, List<Tuple<uint, TValue>>>();

        public void NextGeneration()
        {
            Generation++;
        }

        public bool ContainsKey(TKey k)     // do we have a list of entries under k
        {
            return dictionary.ContainsKey(k);
        }

        public TValue Get(TKey k, uint generation)  // get key, null if not
        {
            TValue v = default(TValue);
            if (dictionary.TryGetValue(k, out List<Tuple<uint, TValue>> list))
            {
                foreach (var t in list)
                {
                    if (t.Item1 <= generation)  // for all generations before and it, its a good value
                    {
                        v = t.Item2;
                        if (t.Item1 == generation)  // stop if we hit generation
                            break;
                    }
                    else
                        break;      // stop, generations are always added in order, and the generation we are on is > one we want
                }
            }
            return v;
        }

        public Dictionary<TKey, TValue> Get(uint generation, Predicate<TValue> predicate = null)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();
            foreach (var kvp in dictionary)
            {
                TValue v = default(TValue);
                foreach (var t in kvp.Value)
                {
                    if (t.Item1 <= generation)  // for all generations before and it, its a good value
                    {
                        v = t.Item2;
                        if (t.Item1 == generation)  // stop if we hit generation
                            break;
                    }
                    else
                        break;      // stop, generations are always added in order, and the generation we are on is > one we want
                }

                if (v != null && (predicate == null || predicate(v)))
                    ret[kvp.Key] = v;
            }

            return ret;
        }

        public List<TValue> GetValues(uint generation, Predicate<TValue> predicate = null)
        {
            List<TValue> ret = new List<TValue>();
            foreach (var kvp in dictionary)
            {
                TValue v = default(TValue);
                foreach (var t in kvp.Value)
                {
                    if (t.Item1 <= generation)  // for all generations before and it, its a good value
                    {
                        v = t.Item2;
                        if (t.Item1 == generation)  // stop if we hit generation
                            break;
                    }
                    else
                        break;      // stop, generations are always added in order, and the generation we are on is > one we want
                }

                if (v != null && (predicate == null || predicate(v)))
                    ret.Add(v);
            }

            return ret;
        }

        public TValue GetLast(TKey k)       // get last entry of key K
        {
            if (dictionary.TryGetValue(k, out List<Tuple<uint, TValue>> list))
            {
                return list[list.Count - 1].Item2;
            }
            else
                return default(TValue);
        }

        public Dictionary<TKey, TValue> GetLast()
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();
            foreach (var kvp in dictionary)
            {
                ret[kvp.Key] = kvp.Value[kvp.Value.Count - 1].Item2;
            }

            return ret;
        }

        public List<TValue> GetLastValues()
        {
            List<TValue> ret = new List<TValue>();
            foreach (var kvp in dictionary)
            {
                ret.Add(kvp.Value[kvp.Value.Count - 1].Item2);
            }
            return ret;
        }

        public List<TKey> GetKeys()
        {
            List<TKey> ret = new List<TKey>();
            foreach (var kvp in dictionary)
            {
                ret.Add(kvp.Key);
            }
            return ret;
        }

        public void Add(TKey k, TValue v)
        {
            if (!dictionary.ContainsKey(k))
                dictionary[k] = new List<Tuple<uint, TValue>>();

            dictionary[k].Add(new Tuple<uint, TValue>(Generation, v));
        }

        public void AddGeneration(TKey k, TValue v)
        {
            Generation++;

            if (!dictionary.ContainsKey(k))
                dictionary[k] = new List<Tuple<uint, TValue>>();

            dictionary[k].Add(new Tuple<uint, TValue>(Generation, v));
        }


    }
}