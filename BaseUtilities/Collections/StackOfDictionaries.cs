/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BaseUtils
{
    public class StackOfDictionaries<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
    {
        private List<Dictionary<TKey, TValue>> dictionarylist;      

        public StackOfDictionaries()
        {
            dictionarylist = new List<Dictionary<TKey, TValue>>();
            dictionarylist.Add(new Dictionary<TKey, TValue>());
        }

        public TValue this[TKey key]
        {
            get
            {
                foreach (var dict in dictionarylist)
                {
                    if (dict.ContainsKey(key))
                        return dict[key];
                }

                return default(TValue);
            }
            set
            {
                dictionarylist.First().Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)  => dictionarylist.First().Add(key, value);  // to top level only

        public void AddBase(TKey key, TValue value)  => dictionarylist[dictionarylist.Count - 1].Add(key, value);  // to base level

        public bool ContainsKey(TKey key)
        {
            foreach (var dict in dictionarylist)
            {
                if (dict.ContainsKey(key))
                    return true;
            }
            return false;
        }

        public bool Remove(TKey key, bool toponly = true)
        {
            if (toponly)
                return dictionarylist.First().Remove(key);
            else
            {
                foreach (var dict in dictionarylist)
                {
                    if (dict.Remove(key))
                        return true;
                }
            }
            return false;
        }

        public int Count
        {
            get
            {
                int c = 0;
                foreach (var dict in dictionarylist)
                    c += dict.Count;
                return c;
            }
        }

        public int Levels => dictionarylist.Count;

        public void Clear() => dictionarylist.Clear();

        public void Push() => dictionarylist.Insert(0, new Dictionary<TKey, TValue>());

        public void Pop()
        {
            if ( dictionarylist.Count > 1 )
                dictionarylist.RemoveAt(0);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(dictionarylist);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(dictionarylist);
        }


        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private List<Dictionary<TKey, TValue>> dictionarylist;
            private int level;
            private Dictionary<TKey, TValue>.Enumerator curpos;

            public Enumerator(List<Dictionary<TKey, TValue>> d)
            {
                dictionarylist = d;
                level = 0;
                curpos = dictionarylist[level].GetEnumerator();
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current { get { return curpos.Current; } }

            object IEnumerator.Current => throw new NotImplementedException();      // not clear on use case - does not get called in foreach

            public void Dispose()
            {
            }

            public bool MoveNext()          // movenext is called before loop begins.. always called at start
            {
                if (!curpos.MoveNext())
                {
                    level++;

                    while (level < dictionarylist.Count && dictionarylist[level].Count == 0)        // skip thru empty levels
                        level++;

                    if (level >= dictionarylist.Count)              // exceeded, stop
                        return false;

                    curpos = dictionarylist[level].GetEnumerator();
                    return curpos.MoveNext();
                }

                return true;
            }

            public void Reset()
            {
                level = 0;
                curpos = dictionarylist[level].GetEnumerator();
            }
        }

    }
}