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
using System.Linq;

namespace BaseUtils.JSON
{
    public static class JTokenExtensions
    {
        public static T ToObject<T>(this JToken tk)         // returns null if not decoded
        {
            return ToObjectProtected<T>(tk,false);
        }

        public static T ToObjectProtected<T>(this JToken tk, bool ignoretypeerrors = false)  // backwards compatible naming
        {
            Type tt = typeof(T);
            Object ret = tk.ToObject(tt,ignoretypeerrors);
            if (ret is ToObjectError)
            {
                System.Diagnostics.Debug.WriteLine("To Object error:" + ((ToObjectError)ret).ErrorString + ":" + ((ToObjectError)ret).PropertyName);
                return default(T);
            }
            else if (ret != null)      // or null
                return (T)ret;          // must by definition have returned tt.
            else
                return default(T);
        }

        public class ToObjectError
        {
            public string ErrorString;
            public string PropertyName;
            public ToObjectError(string s) { ErrorString = s; PropertyName = ""; }
        };

        // returns Object of type tt, or ToObjectError, or null if tk == JNotPresent.  
        // ignoreerrors means don't worry if individual fields are wrong type in json vs in classes/dictionaries

        public static Object ToObject(this JToken tk, Type tt, bool ignoretypeerrors = false)       // will return an instance of tt or ToObjectError, or null for token is null
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
                        Object ret = ToObject(tk[i], tt.GetElementType(), ignoretypeerrors);      // get the underlying element, must match array element type

                        if (ret != null && ret.GetType() == typeof(ToObjectError))      // arrays must be full, any errors means an error
                        {
                            ((ToObjectError)ret).PropertyName = tt.Name + "." + i.ToString() + "." + ((ToObjectError)ret).PropertyName;
                            return ret;
                        }
                        else
                        {
                            dynamic d = tt.GetElementType().ChangeTo(ret);
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
                        Object ret = ToObject(tk[i], types[0], ignoretypeerrors);      // get the underlying element, must match types[0] which is list type

                        if (ret != null && ret.GetType() == typeof(ToObjectError))  // lists must be full, any errors are errors
                        {
                            ((ToObjectError)ret).PropertyName = tt.Name + "." + i.ToString() + "." + ((ToObjectError)ret).PropertyName;
                            return ret;
                        }
                        else
                        {
                            dynamic d = types[0].ChangeTo(ret);
                            instance.Add(d);
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("JSONToObject: Not array");
            }
            else if (tk.TokenType == JToken.TType.Object)                   // objects are best efforts.. fills in as many fields as possible
            {
                if (typeof(System.Collections.IDictionary).IsAssignableFrom(tt))       // if its a Dictionary<x,y> then expect a set of objects
                {
                    dynamic instance = Activator.CreateInstance(tt);        // create the class, so class must has a constructor with no paras
                    var types = tt.GetGenericArguments();

                    foreach (var kvp in (JObject)tk)
                    {
                        Object ret = ToObject(kvp.Value, types[1],ignoretypeerrors);        // get the value as the dictionary type - it must match type or it get OE

                        if (ret != null && ret.GetType() == typeof(ToObjectError))
                        {
                            ((ToObjectError)ret).PropertyName = tt.Name + "." + kvp.Key + "." + ((ToObjectError)ret).PropertyName;

                            if (ignoretypeerrors)
                            {
                                System.Diagnostics.Debug.WriteLine("Ignoring Object error:" + ((ToObjectError)ret).ErrorString + ":" + ((ToObjectError)ret).PropertyName);
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else
                        {
                            dynamic d = types[1].ChangeTo(ret);
                            instance[kvp.Key] = d;
                        }
                    }

                    return instance;
                }
                else if (tt.IsClass ||      // if class 
                         (tt.IsValueType && !tt.IsPrimitive && !tt.IsEnum && tt != typeof(DateTime)))   // or struct, but not datetime (handled below)
                {
                    var instance = Activator.CreateInstance(tt);        // create the class, so class must has a constructor with no paras

                    var allmembers = tt.GetMembers(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                                System.Reflection.BindingFlags.Public);
                    var fieldpropertymembers = allmembers.Where(x => x.MemberType == System.Reflection.MemberTypes.Property || x.MemberType == System.Reflection.MemberTypes.Field).ToArray();

                    string[] memberjsonname = fieldpropertymembers.Select(mi =>
                    {                                                           // go thru each and look for ones with the rename attr
                        var rename = mi.GetCustomAttributes(typeof(JsonNameAttribute), false);
                        if (rename.Length == 1)
                        {
                            dynamic attr = rename[0];               // if so, dynamically pick up the name
                            return (string)attr.Name;
                        }
                        else
                            return mi.Name;
                    }).ToArray();

                    foreach (var kvp in (JObject)tk)
                    {
                        var pos = System.Array.FindIndex(memberjsonname, x => x == kvp.Key);

                        if (pos >= 0)                                   // if we found a class member
                        {
                            var mi = fieldpropertymembers[pos];

                            var ca = mi.GetCustomAttributes(typeof(JsonIgnoreAttribute), false);
                            if (ca.Length == 0)                                              // ignore any ones with JsonIgnore on it.
                            {
                                Type otype = mi.FieldPropertyType();

                                if (otype != null)                          // and its a field or property
                                {
                                    Object ret = ToObject(kvp.Value, otype, ignoretypeerrors);    // get the value - must match otype.. ret may be zero for ? types

                                    if (ret != null && ret.GetType() == typeof(ToObjectError))
                                    {
                                        ((ToObjectError)ret).PropertyName = tt.Name + "." + kvp.Key + "." + ((ToObjectError)ret).PropertyName;

                                        if (ignoretypeerrors)
                                        {
                                            System.Diagnostics.Debug.WriteLine("Ignoring Object error:" + ((ToObjectError)ret).ErrorString + ":" + ((ToObjectError)ret).PropertyName);
                                        }
                                        else
                                        {
                                            return ret;
                                        }
                                    }
                                    else
                                    {
                                        if (!mi.SetValue(instance, ret))         // and set. Set will fail if the property is get only
                                        {
                                            if (ignoretypeerrors)
                                            {
                                                System.Diagnostics.Debug.WriteLine("Ignoring cannot set value on property " + mi.Name);
                                            }
                                            else
                                            {
                                                return new ToObjectError("Cannot set value on property " + mi.Name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("JSONToObject: No such member " + kvp.Key + " in " + tt.Name);
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("JSONToObject: Not class");
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
                else if (tt == typeof(DateTime))
                {
                    DateTime? dt = tk.DateTime(System.Globalization.CultureInfo.InvariantCulture);
                    if (dt != null)
                        return dt;
                }
                else if (tt == typeof(DateTime?))
                {
                    if (tk.IsNull)
                        return null;
                    DateTime? dt = tk.DateTime(System.Globalization.CultureInfo.InvariantCulture);
                    if (dt != null)
                        return dt;
                }
                else if ( tt.IsEnum)
                {
                    if (!tk.IsString)
                        return null;

                    try
                    {
                        Object p = Enum.Parse(tt, tk.Str(), true);
                        return Convert.ChangeType(p, tt);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Unable to convert to enum " + tk.Str());
                        return null;
                    }
                }

                return new ToObjectError("JSONToObject: Bad Conversion " + tk.TokenType + " to " + tt.Name);
            }
        }
    }
}



