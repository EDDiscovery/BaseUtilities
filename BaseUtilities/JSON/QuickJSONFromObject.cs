/*
 * Copyright © 2020 robby & EDDiscovery development team
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
 * EDDiscovery== typeof(not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;

namespace BaseUtils.JSON
{
    public partial class JToken
    {
        // null if can't convert

        public static JToken FromObject(Object o)
        {
            return FromObject(o, false, null);
        }

        public static JToken FromObject(Object o, bool ignoreunserialisable, Type[] ignored)
        {
            Type tt = o.GetType();

            if (tt.IsArray)
            {
                Array b = o as Array;

                JArray outarray = new JArray();

                for (int i = 0; i < b.Length; i++)
                {
                    object oa = b.GetValue(i);
                    JToken inner = FromObject(oa, ignoreunserialisable, ignored);
                    if (inner == null)
                        return null;
                    outarray.Add(inner);
                }

                return outarray;
            }
            else if (typeof(System.Collections.IList).IsAssignableFrom(tt))
            {
                var ilist = o as System.Collections.IList;

                JArray outarray = new JArray();

                foreach (var oa in ilist)
                {
                    JToken inner = FromObject(oa, ignoreunserialisable, ignored);
                    if (inner == null)
                        return null;
                    outarray.Add(inner);
                }

                return outarray;
            }
            else if (typeof(System.Collections.IDictionary).IsAssignableFrom(tt))       // if its a Dictionary<x,y> then expect a set of objects
            {
                System.Collections.IDictionary idict = o as System.Collections.IDictionary;

                JObject outobj = new JObject();

                foreach (KeyValuePair<string, Object> kvp in idict)
                {
                    JToken inner = FromObject(kvp.Value, ignoreunserialisable, ignored);
                    if (inner == null)
                        return null;
                    outobj[kvp.Key] = inner;
                }

                return outobj;
            }
            else if (tt.IsClass && tt != typeof(string))
            {
                JObject outobj = new JObject();

                var members = tt.GetMembers(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                foreach (var mi in members)
                {
                    //System.Diagnostics.Debug.WriteLine("Name " + mi.Name + " " + mi.MemberType);

                    Type innertype = null;

                    if (mi.MemberType == System.Reflection.MemberTypes.Property)
                        innertype = ((System.Reflection.PropertyInfo)mi).PropertyType;
                    else if (mi.MemberType == System.Reflection.MemberTypes.Field)
                        innertype = ((System.Reflection.FieldInfo)mi).FieldType;
                    else
                        continue;

                    if (ignored != null && Array.IndexOf(ignored, innertype) >= 0)
                        continue;

                    Object innervalue = null;
                    if (mi.MemberType == System.Reflection.MemberTypes.Property)
                        innervalue = ((System.Reflection.PropertyInfo)mi).GetValue(o);
                    else 
                        innervalue = ((System.Reflection.FieldInfo)mi).GetValue(o);

                    if ( innervalue != null )
                    {
                        var token = FromObject(innervalue, ignoreunserialisable, ignored);     // may return null if not serializable

                        if (token == null)
                        {
                            if (!ignoreunserialisable)
                                return null;
                        }
                        else
                            outobj[mi.Name] = token;
                    }
                    else
                    {
                        outobj[mi.Name] = JToken.Null();        // its null so its a JNull
                    }
                }

                return outobj;
            }
            else
            {
                return JToken.CreateToken(o, false);        // return token or null indicating unserializable
            }
        }
    }
}



