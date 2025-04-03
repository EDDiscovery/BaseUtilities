/*
 * Copyright © 2017-2020 EDDiscovery development team
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
using QuickJSON.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BaseUtils
{
    public static partial class TypeHelpers
    {
        static public MethodInfo FindMember(this MemberInfo[] methods, Type[] paras)    // Must be MethodInfo's, find matching these paras..
        {
            foreach (var memberinfo in methods)
            {
                MethodInfo mi = (MethodInfo)memberinfo;
                ParameterInfo[] p = mi.GetParameters();
                if (p.Length == paras.Length)
                {
                    int i = 0;
                    for (; i < p.Length; i++)
                    {
                        if (p[i].ParameterType != paras[i])
                            break;
                    }

                    if (i == p.Length)
                        return mi;
                }
            }

            return null;
        }

        static public T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        static public T SafeParseEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                return default(T);
            }
        }

        static public Type FieldPropertyType(this MemberInfo mi)        // from member info for properties/fields return type
        {
            if (mi.MemberType == System.Reflection.MemberTypes.Property)
                return ((System.Reflection.PropertyInfo)mi).PropertyType;
            else if (mi.MemberType == System.Reflection.MemberTypes.Field)
                return ((System.Reflection.FieldInfo)mi).FieldType;
            else
                return null;
        }

        // field or property.
        static public bool TryGetValue<T>(object instance, string fieldorproperty, out T value, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            if (TryGetValue(instance,fieldorproperty, out object valueo) && valueo.GetType() == typeof(T))
            {
                value = (T)valueo;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        static public bool TryGetValue(object instance, string fieldorproperty, out object value, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            MemberInfo mi = instance.GetType().GetField(fieldorproperty, flags);
            if (mi == null)
                mi = instance.GetType().GetProperty(fieldorproperty, flags);
            return TryGetValue(mi, instance, out value);
        }

        static public bool TryGetValue(MemberInfo mi, object instance, out object value)
        {
            if (mi != null)
            {
                if (mi.MemberType == MemberTypes.Property)
                {
                    var pi = (System.Reflection.PropertyInfo)mi;
                    if (pi.GetMethod != null)
                    {
                        value = ((System.Reflection.PropertyInfo)mi).GetValue(instance);
                        return true;
                    }
                }
                else
                {
                    value = ((System.Reflection.FieldInfo)mi).GetValue(instance);
                    return true;
                }
            }
            
            value = null;
            return false;
        }

        // given a member of fields/property, set value in instance
        public static bool SetValue(this MemberInfo mi, Object instance,  Object value)   
        {
            try
            {
                if (mi.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var fi = (System.Reflection.FieldInfo)mi;
                    fi.SetValue(instance, value);       // may except
                    return true;
                }
                else if (mi.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var pi = (System.Reflection.PropertyInfo)mi;
                    if (pi.SetMethod != null)
                    {
                        pi.SetValue(instance, value);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    throw new NotSupportedException();
            }
            catch { }
            {
                return false;
            }

        }

        // cls = class type (such as typeof(JTokenExtensions)).. gentype = <T> parameter.  then Invoke with return.Invoke(null,new Object[] { values..}) if static, null = instance if not
        public static MethodInfo CreateGeneric(Type cls, string methodname, Type gentype)
        {
            System.Reflection.MethodInfo method = cls.GetMethod(methodname, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return method.MakeGenericMethod(gentype);
        }

        public static Object ChangeTo(this Type type, Object value)     // this extends ChangeType to handle nullables.
        {
            Type underlyingtype = Nullable.GetUnderlyingType(type);     // test if its a nullable type (double?)
            if (underlyingtype != null)
            {
                if (value == null)
                    return null;
                else
                    return Convert.ChangeType(value, underlyingtype);
            }
            else
            {
                return Convert.ChangeType(value, type);       // convert to element type, which should work since we checked compatibility
            }
        }

        public static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> items)      // for some reason missing from HashSet
        {
            foreach (var d in items)
                hash.Add(d);
        }

        public static void CopyPropertiesFields(this Object to, Object from, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool properties = true, bool fields = true)
        {
            foreach (MemberInfo mi in from.GetType().GetMembers(bf))
            {
                if (mi.MemberType == MemberTypes.Property && properties)
                {
                    PropertyInfo pi = mi as PropertyInfo;
                    if (pi.CanWrite)
                    {
                        System.Diagnostics.Debug.WriteLine($"TypeHelpers copy property {pi.Name}");
                        pi.SetValue(to, pi.GetValue(from));
                    }
                }
                else if (mi.MemberType == MemberTypes.Field && fields)
                {
                    FieldInfo fi = mi as FieldInfo;
                    var ca = fi.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);     // ignore backing fields of properties by seeing if its a compiler generated
                    if (ca.Length == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"TypeHelpers copy field {fi.Name}");
                        fi.SetValue(to, fi.GetValue(from));
                    }
                }
            }
        }
    }
}
