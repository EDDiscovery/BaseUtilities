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
using System.Linq;
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Resource " + resource + "." + item + " Exception" + e);
            }

            return null;
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        // as an Int Array
        static public int[] GetAssemblyVersionValues(this Assembly aw)
        {
            AssemblyName an = new AssemblyName(aw.FullName);            // offical way to split it
            return new int[4] { an.Version.Major, an.Version.Minor, an.Version.Build, an.Version.Revision };
        }

        static public string GetAssemblyVersionString(this Assembly aw)
        {
            AssemblyName an = new AssemblyName(aw.FullName);
            return an.Version.Major.ToStringInvariant() + "." + an.Version.Minor.ToStringInvariant() + "." + an.Version.Build.ToStringInvariant() + "." + an.Version.Revision.ToStringInvariant();
        }

        // resourcename should be the whole thing - OpenTk.name.
        // null if not found
        public static string GetResourceAsString(this Assembly ass, string resourcename)        
        {
            try
            {
                var stream = ass.GetManifestResourceStream(resourcename);

                // System.Diagnostics.Debug.WriteLine(string.Join(", ", ass.GetManifestResourceNames()));

                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Resource get " + e.ToString());
            }

            return null;
        }

        // Opentk.resourcename.. assembly must be loaded.  Must be an embedded resource.
        // null if not found
        public static string GetResourceAsString(string fullname)       
        {
            int dotpos = fullname.IndexOf('.');
            if (dotpos >= 0)
            {
                Assembly aw = BaseUtils.ResourceHelpers.GetAssemblyByName(fullname.Left(dotpos));
                return aw.GetResourceAsString(fullname);
            }
            return null;
        }

        // Opentk.resourcename.. assembly must be loaded.  Must be an embedded resource.
        // null if not found
        public static System.Drawing.Image GetResourceAsImage(string fullname)       
        {
            int dotpos = fullname.IndexOf('.');
            if (dotpos >= 0)
            {
                try
                {
                    Assembly ass = BaseUtils.ResourceHelpers.GetAssemblyByName(fullname.Left(dotpos));

                    //System.Diagnostics.Debug.WriteLine(string.Join(", ", ass.GetManifestResourceNames()));

                    var rs = ass.GetManifestResourceStream(fullname);
                    if (rs != null)
                        return new System.Drawing.Bitmap(rs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Exception {ex}");
                }
            }
            return null;
        }
    }
}
