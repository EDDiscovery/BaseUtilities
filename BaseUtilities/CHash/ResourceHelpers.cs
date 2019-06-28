/*
 * Copyright © 2017-2019 EDDiscovery development team
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaseUtils
{
    public static class ResourceHelpers
    {
        // given a resource path (TestWebServer.Properties.Resources) from an assembly and a resource name (without the extension) get the object item.
        // the resourceext is the common extension given to the resource path.

        public static Object GetResource(this Assembly ass, string resource, string item, string resourceext = ".resources")
        {
            try
            {
                string final = resource + resourceext;
                using (var st = ass.GetManifestResourceStream(final))
                {
                    if (st != null)
                    {
                        using (ResourceReader rr = new ResourceReader(st))
                        {
                            // you can enumerate, which gives the values directly, but you can't lookup by name.. stupid.
                            //IDictionaryEnumerator dict = rr.GetEnumerator();  while (dict.MoveNext())  System.Diagnostics.Debug.WriteLine("   {0}: '{1}' (Type {2})", dict.Key, dict.Value, dict.Value.GetType().Name);

                            rr.GetResourceData(item, out string restype, out byte[] rawdata);       // will except if not there

                            using (MemoryStream ms = new MemoryStream(rawdata))     // convert to memory stream
                            {
                                BinaryFormatter formatter = new BinaryFormatter();      // resources seem
                                return formatter.Deserialize(ms);   // and deserialise object out
                            }
                        }
                    }
                }
            }
            catch ( Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Resource " + resource + "." + item + " Exception" + e);
            }

            return null;
        }
    }
}
