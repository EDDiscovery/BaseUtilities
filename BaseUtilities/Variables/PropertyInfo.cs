/*
 * Copyright © 2017-2022 EDDiscovery development team
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
        public sealed class PropertyNameAttribute : Attribute 
        {
            public string Text { get; set; }
            public PropertyNameAttribute(string text) { Text = text; }      // set to Null to stop GetPropertyFieldNames listing this property
        }

        [System.Diagnostics.DebuggerDisplay("PNI {Name} {Help} {Comment} {DefaultCondition}")]
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
        // note, this corresponds to how Variables AddPropertiesFieldsOfClass works, thus it belongs with it. Namespace is historical

        static public List<PropertyNameInfo> GetPropertyFieldNames(Type jtype, string prefix = "", BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, 
                                    bool fields = false, int linelen = 80, string comment = null, Type excludedeclaretype = null , Type[] propexcluded = null, 
                                    bool excludearrayslistdict = false, int depth = 5, string classsepar = "_" )       // give a list of properties for a given name
        {
            if (depth < 0)
                return null;

            if (jtype != null)
            {
                List<PropertyNameInfo> ret = new List<PropertyNameInfo>();

                foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties(bf))
                {
                    if ((excludedeclaretype == null || pi.DeclaringType != excludedeclaretype) && (propexcluded == null || !propexcluded.Contains(pi.PropertyType)))
                    {
                        if (pi.GetIndexParameters().GetLength(0) == 0)      // only properties with zero parameters are called
                        {
                            AddToPNI(ret, pi.PropertyType, prefix + pi.Name, pi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bf, fields, linelen, comment, excludedeclaretype, propexcluded, excludearrayslistdict, depth - 1, classsepar);
                        }
                    }
                }

                if (fields)
                {
                    foreach (FieldInfo fi in jtype.GetFields(bf))
                    {
                        if ((excludedeclaretype == null || fi.DeclaringType != excludedeclaretype) && (propexcluded == null || !propexcluded.Contains(fi.FieldType)))
                        {
                            AddToPNI(ret, fi.FieldType, prefix + fi.Name, fi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bf, fields, linelen, comment, excludedeclaretype, propexcluded, excludearrayslistdict, depth - 1, classsepar);
                        }
                    }
                }

                return ret;
            }
            else
                return null;
        }

        static public void AddToPNI(List<PropertyNameInfo> ret, Type pt, string name, object [] ca, BindingFlags bf, bool fields, int linelen, string comment, 
                                                                    Type excludedeclaretype, Type[] propexcluded,bool excludearrayslistdict, int depth, string classsepar)
        {
            string help = ca.Length > 0 ? ((dynamic)ca[0]).Text : "";

            if (help == null)       // this cancels help if present
                return;
            
            if (pt.IsArray)
            {
                if (excludearrayslistdict)
                    return;

                Type arraytype = pt.GetElementType();

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>[1..N]" + classsepar + "itemname for " + help);
                ret.Add(pni);

                if (arraytype != typeof(string))        // don't do strings..
                {
                    var pnis = GetPropertyFieldNames(arraytype, name + "[]" + classsepar, bf, fields, linelen, comment, excludedeclaretype, propexcluded, excludearrayslistdict, depth - 1, classsepar);
                    if (pnis != null)
                        ret.AddRange(pnis);
                }

            }
            else if ((typeof(System.Collections.IDictionary).IsAssignableFrom(pt)))
            {
                if (excludearrayslistdict)
                    return;

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>" + classsepar + "itemname for " + help);
                ret.Add(pni);
            }
            else if (typeof(System.Collections.IList).IsAssignableFrom(pt))
            {
                if (excludearrayslistdict)
                    return;

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>[1..N]" + classsepar + "itemname for " + help);
                ret.Add(pni);

                var subclasslist = GetPropertyFieldNames(pt.GenericTypeArguments[0], name + "[]" + classsepar, bf, fields, linelen, comment, excludedeclaretype, propexcluded, excludearrayslistdict, depth - 1, classsepar);
                if (subclasslist != null)
                    ret.AddRange(subclasslist);
            }
            else if (pt.IsClass && pt != typeof(string))
            {
                var pni = GetPropertyFieldNames(pt, name + classsepar, bf, fields, linelen, comment, excludedeclaretype, propexcluded, excludearrayslistdict, depth - 1, classsepar);
                if (pni != null)
                    ret.AddRange(pni);
            }
            else
            {
                PropertyNameInfo pni = PNI(name, pt, linelen, comment, help);
                ret.Add(pni);
            }
        }

        static public PropertyNameInfo PNI( string name, Type t , int linelen, string comment, string help)
        {
            string pname = t.FullName;

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(t))
            {
                help = ("Dictionary class (" + t.GenericTypeArguments[0].Name + "," + t.GenericTypeArguments[1].Name + ")").AppendPrePad(help, " : ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericGreaterEqual, comment);
            }
            else if (t.IsEnum)
            {
                string[] enums = Enum.GetNames(t);
                help = ("Enumeration: " + enums.FormatIntoLines(linelen)).AppendPrePad(help, Environment.NewLine);
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.Equals, comment);
            }
            else if (pname.Contains("System.Double"))
            {
                help = "Floating point value".AppendPrePad(help, ": ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericGreaterEqual, comment);
            }
            else if (pname.Contains("System.Boolean"))
            {
                help = "Boolean value: 1 = true, 0 = false".AppendPrePad(help, ": ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.IsTrue, comment);
            }
            else if (pname.Contains("System.Int"))
            {
                help = "Integer value".AppendPrePad(help, ": ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.NumericEquals, comment);
            }
            else if (pname.Contains("System.DateTime"))
            {
                help = "Date time value, US format".AppendPrePad(help, ": ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.DateAfter, comment);
            }
            else
            {
                help = "String value".AppendPrePad(help, ": ");
                return new PropertyNameInfo(name, help, ConditionEntry.MatchType.Contains, comment);
            }
        }

    }
}
