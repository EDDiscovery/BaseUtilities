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
        public uint UpdatesAtThisGeneration { get; private set; } = 0;

        private Dictionary<TKey, DictionaryWithFirstLastKey<uint, TValue>> dictionary = new Dictionary<TKey, DictionaryWithFirstLastKey<uint, TValue>>();

        public void NextGeneration()
        {
            Generation++;
            UpdatesAtThisGeneration = 0;
        }

        public void AbandonGeneration()
        {
            Generation--;
            UpdatesAtThisGeneration = 0;
        }

        public bool ContainsKey(TKey k)     // do we have a list of entries under k
        {
            return dictionary.ContainsKey(k);
        }

        public TValue Get(TKey k, uint generation)  // get key, null if not
        {
            TValue v = default(TValue);
            if (dictionary.TryGetValue(k, out DictionaryWithFirstLastKey<uint, TValue> dict))       // try find key, return dictionary list of gens
            {
                if (generation >= dict.LastKey)                                    // if generation is at or greater to last key added, return lastkey
                    return dict[dict.LastKey];

                if (generation < dict.FirstKey )                                    // if generation is less than first key, no result
                    return v;

                do
                {
                    if (dict.TryGetValue(generation, out TValue res))               // in gen list, try find value at generation
                        return res;

                } while (generation-- > 0);                                         // go back in generations until we get to zero, inclusive
            }
            return v;
        }

        public Dictionary<TKey, TValue> Get(uint generation, Predicate<TValue> predicate = null)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();
            foreach (var kvp in dictionary)
            {
                if (generation >= kvp.Value.LastKey)                    // if generation is at or greater to last key added, return lastkey
                {
                    TValue v = kvp.Value[kvp.Value.LastKey];            // get value
                    if (predicate == null || predicate(v))              // no predicate or passed, store it
                        ret[kvp.Key] = v;
                }
                else if (generation >= kvp.Value.FirstKey)              // if generation is greater than first key added, we can find it
                {
                    uint g = generation;
                    do
                    {
                        if (kvp.Value.TryGetValue(g, out TValue v))     // in gen list, try find value at generation, if so, got it
                        {
                            if (predicate == null || predicate(v))      // no predicate or passed, store it
                                ret[kvp.Key] = v;
                            break;
                        }
                    } while (g-- > 0);                                     // go back in generations until we get to zero, inclusive
                }
                else
                {

                }
            }

            return ret;
        }

        public List<TValue> GetValues(uint generation, Predicate<TValue> predicate = null)
        {
            List<TValue> ret = new List<TValue>();
            foreach (var kvp in dictionary)
            {
                if (generation >= kvp.Value.LastKey)                    // if generation is at or greater to last key added, return lastkey
                {
                    TValue v = kvp.Value[kvp.Value.LastKey];            // get value
                    if (predicate == null || predicate(v))              // no predicate or passed, store it
                        ret.Add(v);
                }
                else if (generation >= kvp.Value.FirstKey)              // if generation is greater than first key added, we can find it
                {
                    uint g = generation;
                    do
                    {
                        if (kvp.Value.TryGetValue(g, out TValue v))     // in gen list, try find value at generation, if so, got it
                        {
                            if (predicate == null || predicate(v))      // no predicate or passed, store it
                                ret.Add(v);
                            break;
                        }
                    } while (g-- > 0);                                     // go back in generations until we get to zero, inclusive
                }
            }

            return ret;
        }

        public TValue GetLast(TKey k)       // get last entry of key K
        {
            if (dictionary.TryGetValue(k, out DictionaryWithFirstLastKey<uint, TValue> dict))
            {
                return dict[dict.LastKey];
            }
            else
                return default(TValue);
        }

        public Dictionary<TKey, TValue> GetLast(Predicate<TValue> predicate = null)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();
            if (predicate == null)
            {
                foreach (var kvp in dictionary)
                {
                    ret[kvp.Key] = kvp.Value[kvp.Value.LastKey];
                }
            }
            else
            {
                foreach (var kvp in dictionary)
                {
                    if (predicate(kvp.Value[kvp.Value.LastKey]))
                        ret[kvp.Key] = kvp.Value[kvp.Value.LastKey];
                }
            }

            return ret;
        }

        public List<TValue> GetLastValues(Predicate<TValue> predicate = null)
        {
            List<TValue> ret = new List<TValue>();
            if (predicate == null)
            {
                foreach (var kvp in dictionary)
                {
                    ret.Add(kvp.Value[kvp.Value.LastKey]);
                }
            }
            else
            {
                foreach (var kvp in dictionary)
                {
                    if (predicate(kvp.Value[kvp.Value.LastKey]))
                        ret.Add(kvp.Value[kvp.Value.LastKey]);
                }

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

        public void Add(TKey k, TValue v)       // don't double add
        {
            if (!dictionary.ContainsKey(k))
                dictionary[k] = new DictionaryWithFirstLastKey<uint, TValue>();

            dictionary[k].Add(Generation, v);
            UpdatesAtThisGeneration++;
        }

        public void AddGeneration(TKey k, TValue v)   // don't double add
        {
            NextGeneration();

            if (!dictionary.ContainsKey(k))
                dictionary[k] = new DictionaryWithFirstLastKey<uint, TValue>();

            dictionary[k].Add(Generation, v);
            UpdatesAtThisGeneration++;
        }

    }
}