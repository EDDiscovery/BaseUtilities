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
 *
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


        /// <summary>
        /// given a type, iterate to thru members to collect property type info
        /// note, this corresponds to how Variables AddPropertiesFieldsOfClass works, thus it belongs with it. Namespace is historical
        /// </summary>
        /// <param name="jtype">type to iterate</param>
        /// <param name="prefix">Prefix to put in front of name</param>
        /// <param name="bindingflags">What members to iterate</param>
        /// <param name="fields">if to include fields in any iteration</param>
        /// <param name="linelen">max linelen before wrap</param>
        /// <param name="comment">comment to add at end</param>
        /// <param name="excludedeclaretypes">exclude these declaring type</param>
        /// <param name="excludepropertytypes">exclude these types</param>
        /// <param name="excludearrayslistdict">exclude lists or dictionaries</param>
        /// <param name="depth">recursion depth, <0 stop</param>
        /// <param name="classsepar">class seperator string</param>
        /// <returns></returns>

        static public List<PropertyNameInfo> GetPropertyFieldNames(Type jtype, string prefix = "", BindingFlags bindingflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, 
                                    bool fields = false, int linelen = 80, string comment = null, 
                                    Type[] excludedeclaretypes = null , Type[] excludepropertytypes = null, bool excludearrayslistdict = false,
                                    int depth = 5, string classsepar = "_")       // give a list of properties for a given name
        {
            if (depth < 0)
                return null;

            if (jtype != null)
            {
                List<PropertyNameInfo> ret = new List<PropertyNameInfo>();

                foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties(bindingflags))
                {
                    if ((excludedeclaretypes == null || !excludedeclaretypes.Contains(pi.DeclaringType)) && (excludepropertytypes == null || !excludepropertytypes.Contains(pi.PropertyType)))
                    {
                        if (pi.GetIndexParameters().GetLength(0) == 0)      // only properties with zero parameters are called
                        {
                            AddToPNI(ret, pi.PropertyType, prefix + pi.Name, pi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bindingflags, fields, linelen, 
                                    comment, excludedeclaretypes, excludepropertytypes, excludearrayslistdict, depth - 1, classsepar);
                        }
                    }
                }

                if (fields)
                {
                    foreach (FieldInfo fi in jtype.GetFields(bindingflags))
                    {
                        if ((excludedeclaretypes == null || !excludedeclaretypes.Contains(fi.DeclaringType)) && (excludepropertytypes == null || !excludepropertytypes.Contains(fi.FieldType)))
                        {
                            AddToPNI(ret, fi.FieldType, prefix + fi.Name, fi.GetCustomAttributes(typeof(PropertyNameAttribute), false), bindingflags, fields, linelen, comment, excludedeclaretypes, excludepropertytypes, excludearrayslistdict, depth - 1, classsepar);
                        }
                    }
                }

                return ret;
            }
            else
                return null;
        }

        /// <summary>
        /// Add to PNI
        /// </summary>
        /// <param name="ret">Return array</param>
        /// <param name="propertytype">Type </param>
        /// <param name="name">Name to call it</param>
        /// <param name="customattributes">Custom attributes picked up on member</param>
        /// <param name="bindingflags">What members to iterate</param>
        /// <param name="fields">if to include fields in any iteration</param>
        /// <param name="linelen">max linelen before wrap</param>
        /// <param name="comment">comment to add at end</param>
        /// <param name="excludedeclaretypes">exclude these declaring type</param>
        /// <param name="excludepropertytypes">exclude these types</param>
        /// <param name="excludearrayslistdict">exclude lists or dictionaries</param>
        /// <param name="excludeNullPNA">exclude entries with null PNA</param>
        /// <param name="depth">recursion depth, <0 stop</param>
        /// <param name="classsepar">class seperator string</param>
        static private void AddToPNI(List<PropertyNameInfo> ret, Type propertytype, string name, object [] customattributes, BindingFlags bindingflags, bool fields, int linelen, string comment, 
                                                                    Type[] excludedeclaretypes, Type[] excludepropertytypes, bool excludearrayslistdict,
                                                                    int depth, string classsepar)
        {
            string help = customattributes.Length > 0 ? ((dynamic)customattributes[0]).Text : "";

            if (help == null)       // this cancels help if present
                return;
            
            if (propertytype.IsArray)
            {
                if (excludearrayslistdict)
                    return;

                Type arraytype = propertytype.GetElementType();

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>[1..N]" + classsepar + "itemname for " + help);
                ret.Add(pni);

                if (arraytype != typeof(string))        // don't do strings..
                {
                    var pnis = GetPropertyFieldNames(arraytype, name + "[]" + classsepar, bindingflags, fields, linelen, comment, excludedeclaretypes, excludepropertytypes, excludearrayslistdict, depth - 1, classsepar);
                    if (pnis != null)
                        ret.AddRange(pnis);
                }

            }
            else if ((typeof(System.Collections.IDictionary).IsAssignableFrom(propertytype)))
            {
                if (excludearrayslistdict)
                    return;

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>" + classsepar + "itemname for " + help);
                ret.Add(pni);
            }
            else if (typeof(System.Collections.IList).IsAssignableFrom(propertytype))
            {
                if (excludearrayslistdict)
                    return;

                var pni = PNI(name + classsepar + "Count", typeof(int), 0, comment, "Count of items. Use <name>[1..N]" + classsepar + "itemname for " + help);
                ret.Add(pni);

                var subclasslist = GetPropertyFieldNames(propertytype.GenericTypeArguments[0], name + "[]" + classsepar, bindingflags, fields, linelen, comment, excludedeclaretypes, excludepropertytypes, excludearrayslistdict, depth - 1, classsepar);
                if (subclasslist != null)
                    ret.AddRange(subclasslist);
            }
            else if (propertytype.IsClass && propertytype != typeof(string))
            {
                var pni = GetPropertyFieldNames(propertytype, name + classsepar, bindingflags, fields, linelen, comment, excludedeclaretypes, excludepropertytypes, excludearrayslistdict, depth - 1, classsepar);
                if (pni != null)
                    ret.AddRange(pni);
            }
            else
            {
                PropertyNameInfo pni = PNI(name, propertytype, linelen, comment, help);
                ret.Add(pni);
            }
        }
        
        /// <summary>
        /// Format PNI
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="t">Type</param>
        /// <param name="linelen">Length to wrap</param>
        /// <param name="comment">Comment field</param>
        /// <param name="help">help field</param>
        /// <returns></returns>

        static private PropertyNameInfo PNI( string name, Type t , int linelen, string comment, string help)
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
