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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        static public Type FieldPropertyType(this MemberInfo mi)        // from member info for properties/fields return type
        {
            if (mi.MemberType == System.Reflection.MemberTypes.Property)
                return ((System.Reflection.PropertyInfo)mi).PropertyType;
            else if (mi.MemberType == System.Reflection.MemberTypes.Field)
                return ((System.Reflection.FieldInfo)mi).FieldType;
            else
                return null;
        }

        public static bool SetValue(this MemberInfo mi, Object instance,  Object value)   // given a member of fields/property, set value in instance
        {
            if (mi.MemberType == System.Reflection.MemberTypes.Field)
            {
                var fi = (System.Reflection.FieldInfo)mi;
                fi.SetValue(instance, value);
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
    }
}
