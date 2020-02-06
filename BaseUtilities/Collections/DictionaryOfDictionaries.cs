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
 * EDDiscovery is not affiliated with Frontier Developments plc.
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
    public class DictionaryOfDictionaries<TOuterKey, TKey, TValue> : IEnumerable<KeyValuePair<TOuterKey, Dictionary<TKey, TValue>>>
    {
        private Dictionary<TOuterKey,Dictionary<TKey, TValue>> dictionarylist;      

        public DictionaryOfDictionaries()
        {
            dictionarylist = new Dictionary<TOuterKey, Dictionary<TKey, TValue>>();
        }

        public TValue this[TKey key]
        {
            get
            {
                foreach (var dict in dictionarylist)
                {
                    if (dict.Value.ContainsKey(key))
                        return dict.Value[key];
                }

                return default(TValue);
            }
        }

        Dictionary<TKey,TValue> Dictionary(TOuterKey key)
        {
            return dictionarylist[key];
        }

        public void Add(TOuterKey okey, TKey key, TValue value)
        {
            if (!dictionarylist.ContainsKey(okey))
                dictionarylist.Add(okey, new Dictionary<TKey, TValue>());

            dictionarylist[okey].Add(key, value);  // to top level only
        }

        public bool ContainsOuterKey(TOuterKey key)
        {
            return dictionarylist.ContainsKey(key);
        }

        public bool ContainsKey(TKey key)
        {
            foreach (var dict in dictionarylist)
            {
                if (dict.Value.ContainsKey(key))
                    return true;
            }
            return false;
        }

        public bool Remove(TKey key, bool toponly = true)
        {
            foreach (var dict in dictionarylist)
            {
                if (dict.Value.Remove(key))
                    return true;
            }

            return false;
        }

        public int Count
        {
            get
            {
                int c = 0;
                foreach (var dict in dictionarylist)
                    c += dict.Value.Count;
                return c;
            }
        }

        public void Clear() => dictionarylist.Clear();

        public IEnumerator<KeyValuePair<TOuterKey, Dictionary<TKey, TValue>>> GetEnumerator()
        {
            return new Enumerator(dictionarylist);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(dictionarylist);
        }


        public struct Enumerator : IEnumerator<KeyValuePair<TOuterKey, Dictionary<TKey, TValue>>>
        {
            private Dictionary<TOuterKey, Dictionary<TKey, TValue>> dictionarylist;
            private Dictionary<TOuterKey, Dictionary<TKey, TValue>>.Enumerator curpos;

            public Enumerator(Dictionary<TOuterKey, Dictionary<TKey, TValue>> d)
            {
                dictionarylist = d;
                curpos = dictionarylist.GetEnumerator();
            }

            KeyValuePair<TOuterKey, Dictionary<TKey, TValue>> IEnumerator<KeyValuePair<TOuterKey, Dictionary<TKey, TValue>>>.Current
            { get { return curpos.Current; } }                
                
            object IEnumerator.Current => throw new NotImplementedException();      // not clear on use case - does not get called in foreach

            public void Dispose()
            {
            }

            public bool MoveNext()          // movenext is called before loop begins.. always called at start
            {
                return curpos.MoveNext();
            }

            public void Reset()
            {
                curpos = dictionarylist.GetEnumerator();
            }
        }

    }
}