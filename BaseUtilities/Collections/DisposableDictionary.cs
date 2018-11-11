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
    public class DisposableDictionary<TKey, TValue> : Dictionary<TKey,TValue> where TValue : IDisposable
    {
        public new TValue Add(TKey key , TValue v )
        {
            base.Add(key, v);
            return v;
        }

        public void Dispose()
        {
            foreach (IDisposable r in Values)
                r.Dispose();

            Clear();
        }

        public IDisposable Last(Type t, int c = 1)      // give me the last of type t, with a count..
        {
            var v = Values.ToList();        // horrible.. only way i could think of doing this

            for (int i = v.Count - 1; i >= 0; i--)
            {
                if (v[i].GetType() == t)
                {
                    if (--c == 0)
                        return v[i];
                }
            }

            return null;
        }

    }
}