﻿/*
 * Copyright © 2017-2023 EDDiscovery development team
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
 */

using QuickJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseUtils
{
    [System.Diagnostics.DebuggerDisplay("{Count} Variables")]
    public class Variables : IEvalSymbolHandler
    {
        #region Init

        public Variables()
        {
        }

        public Variables(Variables other)
        {
            values = new Dictionary<string, string>(other.values);
        }

        public Variables(Variables other, Variables other2)      // other can be null, other2 must not be
        {
            if (other == null)
                values = new Dictionary<string, string>(other2.values);
            else
            {
                values = new Dictionary<string, string>(other.values);
                Add(other2);
            }
        }

        public Variables(Variables other, Variables other2, Variables other3)
        {
            values = new Dictionary<string, string>(other.values);
            Add(other2);
            Add(other3);
        }

        public Variables(string s, FromMode fm)     //v=1,v=2 no brackets
        {
            FromString(s, fm);
        }

        public Variables(string s, string value)
        {
            values[s] = value;
        }

        public Variables(string[] s) // name,value,name,value..
        {
            System.Diagnostics.Debug.Assert(s.Length % 2 == 0);
            for (int i = 0; i < s.Length; i += 2)
                values[s[i]] = s[i + 1];
        }

        public Variables(Variables other, string name, string value)
        {
            values = new Dictionary<string, string>(other.values);
            values[name] = value;
        }

        #endregion

        #region Read/Set

        public int Count { get { return values.Count; } }

        public string this[string s] { get { return values[s]; } set { values[s] = value; } }       // can be set NULL

        public IEnumerable<string> NameEnumuerable { get { return values.Keys; } }
        public List<string> NameList { get { return values.Keys.ToList(); } }

        public bool EvalSupportSet { get; set; } = false;       // set for Eval engine to set variables

        public bool Exists(string s) { return values.ContainsKey(s); }
        public bool Contains(string s) { return values.ContainsKey(s); }        // to be more consistent
        public bool TryGet(string s, out string ex) { return values.TryGetValue(s, out ex); }
        // will not complain if not there, so true only if there and its of this value
        public bool Equals(string s, string cmp, StringComparison cs = StringComparison.InvariantCultureIgnoreCase) { return values.ContainsKey(s) && values[s].Equals(cmp,cs); }

        public void Clear() { values.Clear(); }

        public void Delete(string name)
        {
            if (values.ContainsKey(name))
                values.Remove(name);
        }

        public void DeleteWildcard(string name)
        { 
            int wildcard = name.IndexOf('*');
            if (wildcard >= 0)
                name = name.Substring(0, wildcard);

            List<string> removelist = new List<string>();
            foreach (KeyValuePair<string, string> k in values)
            {
                if ((wildcard >= 0 && k.Key.StartsWith(name)) || k.Key.Equals(name))
                    removelist.Add(k.Key);
            }

            foreach (string k in removelist)
                values.Remove(k);
        }

        public void Add(List<Variables> varlist)
        {
            if (varlist != null)
                foreach (Variables d in varlist)
                    Add(d);
        }

        public void Add(Variables[] varlist)
        {
            if (varlist != null)
                foreach (Variables d in varlist)
                    Add(d);
        }

        public void Add(Variables d)
        {
            if (d != null)
                Add(d.values);
        }

        public void Add(Dictionary<string, string> list)
        {
            if (list != null)
            {
                foreach (KeyValuePair<string, string> v in list)
                    values[v.Key] = v.Value;
            }
        }

        public int GetInt(string name, int def = 0)     // get or default
        {
            int i;
            if (values.ContainsKey(name) && values[name].InvariantParse(out i))
                return i;
            else
                return def;
        }
        public double GetDouble(string name, double def = 0)     // get or default
        {
            double i;
            if (values.ContainsKey(name) && values[name].InvariantParse(out i))
                return i;
            else
                return def;
        }

        public string GetString(string name, string def = null, bool checklen = false)      // optional check length
        {
            if (values.ContainsKey(name))
            {
                if (!checklen || values[name].Length > 0)
                    return values[name];
            }

            return def;
        }

        public void SetOrRemove(bool add, string name, string value)     // Set it, or remove it
        {
            if (add)
                values[name] = value;
            else
                values.Remove(name);
        }

        public string AddToVar(string name, int add, int initial)       // DOES NOT set anything.. looks up a value and returns +add to it if its numeric.
        {
            if (values.ContainsKey(name))
            {
                int i;
                if (values[name].InvariantParse(out i))
                {
                    return (i + add).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return initial.ToString();
        }

        // return a list just with the names matching filter, or filter*

        public Variables FilterVars(string filter)
        {
            int wildcard = filter.IndexOf('*');
            if (wildcard >= 0)
                filter = filter.Substring(0, wildcard);

            Variables ret = new Variables();

            foreach (KeyValuePair<string, string> k in values)
            {
                if ((wildcard >= 0 && k.Key.StartsWith(filter)) || k.Key.Equals(filter))
                    ret[k.Key] = k.Value;
            }

            return ret;
        }

        #endregion

        #region Input/Output

        // Print vars, if altops is passed in, you can output using alternate operators

        public string ToString(Dictionary<string, string> altops = null, string pad = "", string separ = ",", string prefix = "", 
                                bool bracket = false, bool comma = true, bool space = true)
        {
            string s = "";
            foreach (KeyValuePair<string, string> v in values)
            {
                if (s.Length > 0)
                    s += separ;

                string vs = v.Value.QuoteString(comma: comma, bracket: bracket, space: space);

                if (altops == null)
                    s += prefix + v.Key + pad + "=" + pad + vs;
                else
                {
                    System.Diagnostics.Debug.Assert(altops.ContainsKey(v.Key));
                    s += prefix + v.Key + pad + altops[v.Key] + pad + vs;
                }
            }

            return s;
        }

        public enum FromMode { OnePerLine, MultiEntryComma, MultiEntryCommaBracketEnds };

        public bool FromString(string s, FromMode fm)    // string, not bracketed.
        {
            BaseUtils.StringParser p = new BaseUtils.StringParser(s);
            return FromString(p, fm);
        }

        // FromMode controls where its stopped. 
        // altops enables operators other than = to be used (set/let only) 

        public bool FromString(BaseUtils.StringParser p, FromMode fm, Dictionary<string, string> altops = null)
        {
            Dictionary<string, string> newvars = ReadFromString(p, fm, altops);
            if (newvars != null)
                values = newvars;
            return (newvars != null);
        }

        public bool AddFromString(BaseUtils.StringParser p, FromMode fm, Dictionary<string, string> altops = null)
        {
            Dictionary<string, string> newvars = ReadFromString(p, fm, altops);
            if (newvars != null)
                Add(newvars);
            return (newvars != null);
        }

        private Dictionary<string, string> ReadFromString(BaseUtils.StringParser p, FromMode fm, Dictionary<string, string> altops = null)
        {
            Dictionary<string, string> newvars = new Dictionary<string, string>();

            while (!p.IsEOL)
            {
                string varname = p.NextQuotedWord( "= ");

                if (varname == null)
                    return null;

                if (altops!=null)            // with extended ops, the ops are returned in the altops function, one per variable found
                {                           // used only with let and set..
                    if (varname.EndsWith("$+"))
                    {
                        varname = varname.Substring(0, varname.Length - 2);
                        altops[varname] = "$+=";
                    }
                    else if (varname.EndsWith("$"))
                    {
                        varname = varname.Substring(0, varname.Length - 1);
                        altops[varname] = "$=";
                    }
                    else if (varname.EndsWith("+"))
                    {
                        varname = varname.Substring(0, varname.Length - 1);
                        altops[varname] = "+=";
                    }
                    else
                    {                                           
                        altops[varname] = "=";              // varname is good, it ended with a = or space, default is =

                        bool dollar = p.IsCharMoveOn('$'); // check for varname space $+
                        bool add = p.IsCharMoveOn('+');

                        if (dollar && add)
                            altops[varname] = "$+=";
                        else if ( dollar )
                            altops[varname] = "$=";
                        else if ( add )
                            altops[varname] = "+=";
                    }
                }

                if (!p.IsCharMoveOn('='))
                    return null;

                string value = (fm == FromMode.OnePerLine) ? p.NextQuotedWordOrLine() : p.NextQuotedWord((fm == FromMode.MultiEntryComma) ? ", " : ",) "); 

                if (value == null)
                    return null;

                newvars[varname] = value;

                if (fm == FromMode.MultiEntryCommaBracketEnds && p.PeekChar() == ')')        // bracket, stop don't remove.. outer bit wants to check its there..
                    return newvars;
                else if (fm == FromMode.OnePerLine && !p.IsEOL)        // single entry, must be eol now
                    return null;
                else if (!p.IsEOL && !p.IsCharMoveOn(','))   // if not EOL, but not comma, incorrectly formed list
                    return null;
            }

            return newvars;
        }

        #endregion

        #region Object values to this class

        // For all in the Values list, fill in data given from fields in O if possible
        // stoptext allows you to isolate the root part. Normally ["_","["]

        public void GetValuesIndicated(Object o, Type[] propexcluded, int maxdepth, string[] stoptext)
        {
            Type jtype = o.GetType();

            foreach ( string keyname in values.Keys.ToList())
            {
                string rootname = keyname.Substring(0, keyname.IndexOfOrLength(stoptext));        // cut off the [ and class stuff to get to the root name

                //System.Diagnostics.Debug.WriteLine($"Indicated value {keyname} rootname {rootname}");

                System.Reflection.PropertyInfo pi = jtype.GetProperty(rootname);        // check property for root name, public only
                if ( pi != null )
                {
                    if (propexcluded == null || !propexcluded.Contains(pi.PropertyType))
                    {
                        System.Reflection.MethodInfo getter = pi.GetGetMethod();
                        AddDataOfType(getter.Invoke(o, null), pi.PropertyType, rootname, maxdepth, propexcluded);
                    }
                }
                else
                {
                    System.Reflection.FieldInfo fi = jtype.GetField(rootname);          // check field for root name, public only
                    if ( fi != null )
                    {
                        if (propexcluded == null || !propexcluded.Contains(fi.FieldType))
                        {
                            AddDataOfType(fi.GetValue(o), fi.FieldType, rootname, maxdepth, propexcluded);
                        }
                    }
                }
            }
        }

        // of a class, enumerate and store values in variables
        // prefix is used on all variables created
        // propexcluded knocks out types selected
        // maxdepth of recursion
        // onlyenumerate means pick only top level fields/properties of name
        // ensuredoublerep means a double will always have a . in it, to make sure the reader knows its a double

        public void AddPropertiesFieldsOfClass( Object o, string prefix , Type[] propexcluded, 
                                                int maxdepth, HashSet<string> onlyenumerate = null, bool ensuredoublerep = false, string classsepar = "_",
                                                System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)      
        {
            Type jtype = o.GetType();

            foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties(bf))
            {
                if (onlyenumerate == null || onlyenumerate.Contains(pi.Name))
                {
                   // System.Diagnostics.Debug.WriteLine($"Prop {pi.Name}");
                    if (pi.GetIndexParameters().GetLength(0) == 0 && (propexcluded == null || !propexcluded.Contains(pi.PropertyType)))      // only properties with zero parameters are called
                    {
                        string name = prefix + pi.Name;
                        System.Reflection.MethodInfo getter = pi.GetGetMethod();
                        AddDataOfType(getter.Invoke(o, null), pi.PropertyType, name, maxdepth, propexcluded, ensuredoublerep, classsepar, bf);
                    }
                }
            }

            foreach (System.Reflection.FieldInfo fi in jtype.GetFields(bf))
            {
                if (onlyenumerate == null || onlyenumerate.Contains(fi.Name))
                {
                   // System.Diagnostics.Debug.WriteLine($"Field {fi.Name}");
                    if (propexcluded == null || !propexcluded.Contains(fi.FieldType))
                    {
                        string name = prefix + fi.Name;
                        AddDataOfType(fi.GetValue(o), fi.FieldType, name, maxdepth, propexcluded, ensuredoublerep, classsepar, bf);
                    }
                }
            }
        }

        public void AddDataOfType(Object o, Type rettype, string name, int depth, Type[] classtypeexcluded = null , bool ensuredoublerep = false, string classsepar = "_",
                                    System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
        {
            if (depth < 0)      // 0, list, class, object, .. limit depth
                return;

            if (classtypeexcluded != null && classtypeexcluded.Contains(rettype))
                return;

            if (rettype.IsGenericType && rettype.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                rettype = rettype.GetGenericArguments()[0];
            }

           // System.Diagnostics.Debug.WriteLine("Object " + name + " " + rettype.Name);

            System.Globalization.CultureInfo ct = System.Globalization.CultureInfo.InvariantCulture;

            try // just to make sure a strange type does not barfe it
            {
                if (typeof(System.Collections.IDictionary).IsAssignableFrom(rettype))
                {
                    int count = 0;
                    if (o != null)
                    {
                        var data = (System.Collections.IDictionary)o;           // lovely to work out

                        count = data.Count;

                        foreach (Object k in data.Keys)
                        {
                            if (k is string)
                            {
                                Object v = data[k as string];
                                AddDataOfType(v, v.GetType(), name + classsepar + (string)k, depth - 1, classtypeexcluded, ensuredoublerep, classsepar, bf);
                            }
                        }
                    }

                    values[name + classsepar + "Count"] = values[name + "Count"] = count.ToString(ct);                           // older style for scripts
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(rettype))        // this includes Arrays
                {
                    int count = 0;
                    if (o != null)
                    {
                        var data = (System.Collections.IList)o;           // lovely to work out

                        count = data.Count;

                        for (int i = 0; i < data.Count; i++)
                        {
                            string subname = name + "[" + (i + 1).ToString(ct) + "]";
                            if (data[i] != null)        // if may be null, double check or crash!
                                AddDataOfType(data[i], data[i].GetType(), subname, depth - 1, classtypeexcluded, ensuredoublerep, classsepar, bf);
                            else
                                values[subname] = "";

                        }
                    }

                    values[name + "Count"] = values[name + classsepar + "Count"] = count.ToString(ct);       // Better form to make it more compatible. PNI reports this.
                }
                else if (o == null)
                {
                    values[name] = "";
                }
                else if (o is string)     // string is a class, so intercept first
                {
                    values[name] = o as string;
                }
                else if (rettype.IsClass)
                {
                    foreach (System.Reflection.PropertyInfo pi in rettype.GetProperties(bf))
                    {
                        //  System.Diagnostics.Debug.WriteLine($" Member {pi.Name} pub {pi.PropertyType.IsPublic} npub {pi.PropertyType.IsNestedPublic}");

                        // only properties with zero parameters are called and with public or nested public access
                        if (pi.GetIndexParameters().GetLength(0) == 0 )      
                        {
                            System.Reflection.MethodInfo getter = pi.GetGetMethod();
                            AddDataOfType(getter.Invoke(o, null), pi.PropertyType, name + classsepar + pi.Name , depth-1, classtypeexcluded, ensuredoublerep, classsepar, bf);
                        }
                    }

                    foreach (System.Reflection.FieldInfo fi in rettype.GetFields(bf))
                    {
                        AddDataOfType(fi.GetValue(o), fi.FieldType, name + classsepar + fi.Name, depth-1 , classtypeexcluded, ensuredoublerep, classsepar, bf);
                    }
                }
                else if (rettype.IsPrimitive || rettype == typeof(DateTime))
                {
                    var v = Convert.ChangeType(o, rettype);

                    if (v is double)
                    {
                        string vt = ensuredoublerep ? ((double)v).ToStringG17InvariantWithDot() : ((double)v).ToString(ct);
                        values[name] = vt;
                    }
                    else if (v is int)
                        values[name] = ((int)v).ToString(ct);
                    else if (v is long)
                        values[name] = ((long)v).ToString(ct);
                    else if (v is bool)
                        values[name] = ((bool)v) ? "1" : "0";
                    else if (v is DateTime)
                        values[name] = ((DateTime)v).ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                    else
                        values[name] = v.ToString();
                }
                else
                {
                    values[name] = o.ToString();
                }
            }
            catch { }
        }

        #endregion

        #region JSON to variables and back

        // Recorded 27/10/24 to new syntax output format
        // give root name to start.. or name = null/empty no name
        public void FromJSON(JToken t, string name)     
        {
            //System.Diagnostics.Debug.WriteLine(t.GetType().Name+ " " + name );

            if (t is JArray)
            {
                values[name + "[]_Count"] = t.Count().ToString();
                int childindex = 1;
                foreach (var subitem in t)
                    FromJSON(subitem, name + "[" + (childindex++).ToStringInvariant() + "]");
            }
            else if (t is JObject)
            {
                foreach (var kvp in (JObject)t)
                    FromJSON(kvp.Value, (name.HasChars() ? (name + "." ) : "" ) + kvp.Key);
            }
            else
            {
                if (t.IsBool)       // intercept bool to print 1/0 not true/false, and mark the name as _BOOL to indicate that, and to allow TOJSON to round trip it
                    values[name+"_BOOL"] = ((bool)t) ? "1" : "0";
                else 
                    values[name] = t.ToStringLiteral();
            }
        }
        
        // reverse method of variables -> JSON. Null if no variables
        public JToken ToJSON(string name)     // give root name to start..
        {
            string[] varlist = values.Keys.Where(x => x.StartsWith(name)).ToArray();

            if (varlist.Length > 0)
            {
                int vpos = 0;
                var tk = ToJSONInternal(varlist, ref vpos, name);

               // System.Diagnostics.Debug.WriteLine($"JSON is {tk.Item2.ToString(true)}");

                return tk.Item2;
            }
            else
                return null;
        }

        // iterate thru variables making up json. Variables should be in JSON order, as per output from AddJSONVariables
        private Tuple<string,JToken> ToJSONInternal(string[] varlist, ref int vpos, string vroot)
        {
            string vname = varlist[vpos];
            int nextdelim = vname.IndexOfAny(new char[] { '.', '[' },vroot.Length);

            if (nextdelim == -1)            // if nothing left, its a jtoken, use implicit converstion string->JTOKEN
            {
                string subname = vname.Substring(vroot.Length);

                // its a string, its lost its type, see if we can represent it as a number

                string v = values[varlist[vpos++]];
                long? iv = v.InvariantParseIntNull();
                if (iv.HasValue)        // its a number..
                {
                    if (subname.EndsWith("_BOOL"))      // bools are turned into numbers with _BOOL on the end (see above)
                        return new Tuple<string, JToken>(subname.Substring(0,subname.Length-5), iv != 0);       // turn back to BOOL
                    else
                        return new Tuple<string, JToken>(subname, iv);       // keep as long
                }

                ulong ? ulv = v.InvariantParseULongNull();  // ulong
                if (ulv.HasValue)
                    return new Tuple<string, JToken>(subname, ulv);

                double? dv = v.InvariantParseDoubleNull();  // double
                if (dv.HasValue)
                    return new Tuple<string, JToken>(subname, dv);

                return new Tuple<string, JToken>(subname, v);   // string
            }
            else if (vname[nextdelim] == '.')       // if its an object..
            {
                string newvroot = vname.Substring(0, nextdelim + 1);      // new vroot including delim

                JObject jo = new JObject();
                while (vpos < varlist.Length && varlist[vpos].StartsWith(newvroot))       // while matching new vroot entirely
                {
                    var ret = ToJSONInternal(varlist, ref vpos, newvroot);
                    jo[ret.Item1] = ret.Item2;
                }
                return new Tuple<string, JToken>(vname.Substring(vroot.Length, nextdelim - vroot.Length), jo);
            }
            else
            {           // must be an array
                string newvroot = vname.Substring(0, nextdelim + 1);      // new vroot including delim

                JArray ja = new JArray();
                while (vpos < varlist.Length && varlist[vpos].StartsWith(newvroot))       // while matching new vroot entirely
                {
                    var ret = ToJSONInternal(varlist, ref vpos, newvroot);      // name will be returning ]_Count,1], 2] etc. We ignore names
                    if ( !ret.Item1.Contains("Count"))
                        ja.Add(ret.Item2);
                }
                return new Tuple<string, JToken>(vname.Substring(vroot.Length, nextdelim - vroot.Length), ja);
            }
        }

        #endregion

        #region Eval Support

        public string Qualify(string instr)     // Variables are passed thru this in case we want to do some syntax nerfing, but for now, its just pass back
        {
            return instr;
        }

        // Eval Symbol Get. If not there, ConvertError
        // tries to convert to double/long else return string
        public object Get(string str)
        {
            string qualname = Qualify(str);

            if (Exists(qualname))        //  if we have a variable
            {
                //System.Diagnostics.Debug.WriteLine($"Variables Get {qualname} {values[qualname]}");
                string text = this[qualname];
                if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
                {
                    if (long.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long v))    // if its a number, return number
                        return v;
                    else
                        return d;
                }
                else
                    return text;    // else its a string
            }
            else
                return new StringParser.ConvertError("Unknown symbol " + qualname);
        }

        // Eval Symbol Set - won't be called unless EvalSupportSet
        public object Set(string text, object e)
        {
            this[text] = Convert.ToString(e);
            return e;
        }

        #endregion

        #region vars

        private Dictionary<string, string> values = new Dictionary<string, string>();

        #endregion


    }
}
