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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;

namespace BaseUtils.JSON
{
    public static class JTokenExtensions
    {
        public static T ToObject<T>(this JToken tk)         // returns null if not decoded
        {
            return ToObjectProtected<T>(tk);
        }

        public static T ToObjectProtected<T>(this JToken tk)  // backwards compatible naming
        {
            Type tt = typeof(T);
            Object ret = tk.ToObject(tt);
            if (ret is ToObjectError)
            {
                System.Diagnostics.Debug.WriteLine("To Object error:" + ((ToObjectError)ret).ErrorString);
                return default(T);
            }
            else if (ret != null)      // or null
                return (T)ret;          // must by definition have returned tt.
            else
                return default(T);
        }

        public class ToObjectError { public string ErrorString; public ToObjectError(string s) { ErrorString = s; } };

        // returns Object of type tt, or ToObjectError, or null if tk == JNotPresent.

        public static Object ToObject(this JToken tk, Type tt)       // will return an instance of tt or ToObjectError, or null for token is null
        {
            if (tk == null)
            {
                return null;
            }
            else if (tk.IsArray)
            {
                JArray jarray = (JArray)tk;

                if (tt.IsArray)
                {
                    dynamic instance = Activator.CreateInstance(tt, tk.Count);   // dynamic holder for instance of array[]

                    for (int i = 0; i < tk.Count; i++)
                    {
                        Object ret = ToObject(tk[i], tt.GetElementType());      // get the underlying element, must match array element type

                        if (ret.GetType() == typeof(ToObjectError))
                            return ret;
                        else
                        {
                            dynamic d = Convert.ChangeType(ret, tt.GetElementType());       // convert to element type, which should work since we checked compatibility
                            instance[i] = d;
                        }
                    }

                    return instance;
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(tt))
                {
                    dynamic instance = Activator.CreateInstance(tt);        // create the List
                    var types = tt.GetGenericArguments();

                    for (int i = 0; i < tk.Count; i++)
                    {
                        Object ret = ToObject(tk[i], types[0]);      // get the underlying element, must match types[0] which is list type

                        if (ret.GetType() == typeof(ToObjectError))
                            return ret;
                        else
                        {
                            dynamic d = Convert.ChangeType(ret, types[0]);       // convert to element type, which should work since we checked compatibility
                            instance.Add(d);
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("Not array");
            }
            else if (tk.TokenType == JToken.TType.Object)                   // objects are best efforts.. fills in as many fields as possible
            {
                if (typeof(System.Collections.IDictionary).IsAssignableFrom(tt))       // if its a Dictionary<x,y> then expect a set of objects
                {
                    dynamic instance = Activator.CreateInstance(tt);        // create the class, so class must has a constructor with no paras
                    var types = tt.GetGenericArguments();

                    foreach (var kvp in (JObject)tk)
                    {
                        Object ret = ToObject(kvp.Value, types[1]);        // get the value as the dictionary type - it must match type or it get OE

                        if (ret.GetType() == typeof(ToObjectError))
                            return ret;
                        else
                        {
                            dynamic d = Convert.ChangeType(ret, types[1]);       // convert to element type, which should work since we checked compatibility
                            instance[kvp.Key] = d;
                        }
                    }

                    return instance;
                }
                else if (tt.IsClass)
                {
                    var instance = Activator.CreateInstance(tt);        // create the class, so class must has a constructor with no paras

                    var members = tt.GetMembers(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                                System.Reflection.BindingFlags.Public);

                    foreach (var kvp in (JObject)tk)
                    {
                        var pos = System.Array.FindIndex(members, x => x.Name == kvp.Key);

                        if (pos >= 0)                                   // if we found a class member
                        {
                            var mi = members[pos];

                            var ca = mi.GetCustomAttributes(typeof(JsonIgnoreAttribute), false);
                            if (ca.Length == 0)                                              // ignore any ones with JsonIgnore on it.
                            {
                                Type otype = mi.FieldPropertyType();

                                if (otype != null)                          // and its a field or property
                                {
                                    Object ret = ToObject(kvp.Value, otype);    // get the value - must match otype.. ret may be zero for ? types

                                    if (ret != null && ret.GetType() == typeof(ToObjectError))
                                        return ret;
                                    else
                                    {
                                        if (!mi.SetValue(instance, ret))         // and set. Set will fail if the property is get only
                                        {
                                            return new ToObjectError("Cannot set value on property " + mi.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("Not class");
            }
            else
            {
                if (tt == typeof(int))
                {
                    var ret = (int?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(int?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (int?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(long))
                {
                    var ret = (long?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(long?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (long?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(uint))
                {
                    var ret = (uint?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(uint?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (uint?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(ulong))
                {
                    var ret = (ulong?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(ulong?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (ulong?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(double))
                {
                    var ret = (double?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(double?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (double?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(float))
                {
                    var ret = (float?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(float?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (float?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(bool))
                {
                    var ret = (bool?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(bool?))
                {
                    if (tk.IsNull)
                        return null;
                    var ret = (bool?)tk;
                    if (ret.HasValue)
                        return ret.Value;
                }
                else if (tt == typeof(string))
                {
                    if (tk.IsNull)
                        return null;
                    var str = (string)tk;
                    if (str != null)
                        return str;
                }

                return new ToObjectError("Bad Conversion " + tk.TokenType);
            }
        }
    }
}



