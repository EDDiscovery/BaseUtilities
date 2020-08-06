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
using System.Collections.Generic;
using System.Reflection;

namespace BaseUtils
{
    public static class TypeHelpers
    {
        public class PropertyNameInfo
        {
            public string Name;

            public ConditionEntry.MatchType? DefaultCondition;      // null if don't force, else condition
            public string Help;
            public string Comment;

            public PropertyNameInfo() { }
            public PropertyNameInfo(string name, string help, ConditionEntry.MatchType? defcondition = null, string comment = null)
            {
                Name = name; DefaultCondition = defcondition; Help = help; Comment = comment;
            }
        }

        // bf default is DefaultLookup in the .net code for GetProperties()
        static public List<PropertyNameInfo> GetPropertyFieldNames(Type jtype, string prefix = "", BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, 
                                    bool fields = false, int linelen = 80, string comment = null, Type excludedeclaretype = null )       // give a list of properties for a given name
        {
            if (jtype != null)
            {
                List<PropertyNameInfo> ret = new List<PropertyNameInfo>();

                foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties(bf))
                {
                    if (excludedeclaretype == null || pi.DeclaringType != excludedeclaretype)
                    {
                        if (pi.GetIndexParameters().GetLength(0) == 0)      // only properties with zero parameters are called
                        {
                            PropertyNameInfo pni = PNI(prefix + pi.Name, pi.PropertyType, linelen, comment);
                            ret.Add(pni);
                            //    System.Diagnostics.Debug.WriteLine("Prop " + pi.Name + " " + pi.PropertyType.FullName);
                        }
                    }
                }

                if (fields)
                {
                    foreach (FieldInfo fi in jtype.GetFields())
                    {
                        if (excludedeclaretype == null || fi.DeclaringType != excludedeclaretype)
                        {
                            PropertyNameInfo pni = PNI(prefix + fi.Name, fi.FieldType, linelen, comment);
                            ret.Add(pni);
                            //    System.Diagnostics.Debug.WriteLine("Fields " + fi.Name + " " + fi.FieldType.FullName);
                        }
                    }
                }

                return ret;
            }
            else
                return null;
        }

        static public PropertyNameInfo PNI( string name, Type t , int ll, string comment)
        {
            string pname = t.FullName;
            if (t.IsEnum)
            {
                string[] enums = Enum.GetNames(t);
                return new PropertyNameInfo(name, "Enumeration:" + enums.FormatIntoLines(ll), ConditionEntry.MatchType.Equals, comment);
            }
            else if (pname.Contains("System.Double"))
                return new PropertyNameInfo(name, "Floating point value", ConditionEntry.MatchType.NumericGreaterEqual, comment);
            else if (pname.Contains("System.Boolean"))
                return new PropertyNameInfo(name, "Boolean value, 1 = true, 0 = false", ConditionEntry.MatchType.IsTrue, comment);
            else if (pname.Contains("System.Int"))
                return new PropertyNameInfo(name, "Integer value", ConditionEntry.MatchType.NumericEquals, comment);
            else if (pname.Contains("System.DateTime"))
                return new PropertyNameInfo(name, "Date Time Value, US format", ConditionEntry.MatchType.DateAfter, comment);
            else
                return new PropertyNameInfo(name, "String value", ConditionEntry.MatchType.Contains, comment);
        }

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
    }
}
