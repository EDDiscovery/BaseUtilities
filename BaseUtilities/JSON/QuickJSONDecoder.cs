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
using System.Collections;
using System.Collections.Generic;

namespace BaseUtils.JSON
{
    public interface IJType
    {
    }

    public class JObject : IJType, IEnumerable<KeyValuePair<string, Object>>
    {
        public JObject()
        {
            Objects = new Dictionary<string, Object>();
        }

        public Dictionary<string, Object> Objects { get; set; }

        public Object this[string key] { get { return Objects[key]; } set { QuickJsonDecoder.Verify(value); Objects[key] = value; } }
        public bool ContainsKey(string n) { return Objects.ContainsKey(n); }
        public int Count() { return Objects.Count; }
        public bool Remove(string key) { return Objects.Remove(key); }
        public void Clear() { Objects.Clear(); }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        public string String(string name)
        {
            return Objects.ContainsKey(name) && Objects[name] is string ? Objects[name] as string : null;
        }

        public string String(string name, string defvalue)
        {
            return Objects.ContainsKey(name) && Objects[name] is string ? Objects[name] as string : defvalue;
        }

        public int? Int(string name)
        {
            if (Objects.ContainsKey(name) && Objects[name] is long)
                return (int)((long)Objects[name]);
            else
                return null;
        }

        public int Int(string name, int defvalue)
        {
            if (Objects.ContainsKey(name) && Objects[name] is long)
                return (int)((long)Objects[name]);
            else
                return defvalue;
        }

        public long? Long(string name)
        {
            if (Objects.ContainsKey(name) && Objects[name] is long)
                return (long)Objects[name];
            else
                return null;
        }

        public long Long(string name, long defvalue)
        {
            if (Objects.ContainsKey(name) && Objects[name] is long)
                return (long)Objects[name];
            else
                return defvalue;
        }

        public double? Double(string name)
        {
            if (Objects.ContainsKey(name))
            {
                if (Objects[name] is double)
                    return (double)Objects[name];
                else if (Objects[name] is long)
                    return (double)(long)Objects[name];
            }

            return null;
        }

        public double Double(string name, double defvalue)
        {
            double? v = Double(name);
            if (v.HasValue)
                return v.Value;
            else
                return defvalue;
        }

        public DateTime? DateTime(string name, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            string s = String(name);
            if (s != null && System.DateTime.TryParse(s, ci, ds, out DateTime ret))
                return ret;
            else
                return null;
        }

        public DateTime DateTime(string name, DateTime defvalue, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            string s = String(name);
            if (s != null && System.DateTime.TryParse(s, ci, ds, out DateTime ret))
                return ret;
            else
                return defvalue;
        }

        public string ToString(bool verbose = false, string pad = "  ") { return QuickJsonDecoder.ToString(this, verbose, pad); }

        public static JObject Parse(string s)
        {
            Object o = QuickJsonDecoder.Parse(s);
            return o as JObject;    // null if not!
        }
    }

    public class JArray : IJType, IEnumerable<Object>
    {
        public JArray()
        {
            Elements = new List<Object>();
        }

        public List<Object> Elements { get; set; }

        public int Count() { return Elements.Count; }
        public void Add(Object o) { QuickJsonDecoder.Verify(o);  Elements.Add(o); }
        public void AddRange(IEnumerable<Object> o) { Elements.AddRange(o); }
        public void RemoveAt(int index) { Elements.RemoveAt(index); }
        public void Clear() { Elements.Clear(); }

        public List<string> String() { return Elements.ConvertAll<string>((o) => { return o as string; }); }
        public List<int> Int() { return Elements.ConvertAll<int>((o) => { return (int)o; }); }
        public List<long> Long() { return Elements.ConvertAll<long>((o) => { return (long)o; }); }
        public List<double> Double() { return Elements.ConvertAll<double>((o) => { return (double)o; }); }

        public IEnumerator<Object> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        public string ToString(bool verbose = false, string pad = "  ") { return QuickJsonDecoder.ToString(this, verbose, pad); }

        public static JArray Parse(string s)
        {
            Object o = QuickJsonDecoder.Parse(s);
            return o as JArray;    // null if not!
        }
    }

    public class JNull : IJType
    {
    }

    public class QuickJsonDecoder
    {
        public StringParser LineParser { get; set; }        // you can use this to find where the error was
        public bool IsEOL { get { return LineParser.IsEOL; } }    // use this after a decode to see if any text is remaining

        public QuickJsonDecoder()
        {
        }

        public QuickJsonDecoder(string s)
        {
            LineParser = new StringParser(s);
        }

        public static object Parse(string s)        // null if failed.
        {
            QuickJsonDecoder qjd = new QuickJsonDecoder(s);
            return qjd.Decode();
        }

        public static object ParseCheckEOL(string s)        // null if failed - must not be extra text
        {
            QuickJsonDecoder qjd = new QuickJsonDecoder(s);
            Object res = qjd.Decode();
            return qjd.LineParser.IsEOL ? res : null;
        }

        // null if its unhappy
        // decoder does not worry about extra text after the object.  Check LineParser if you are

        public Object Decode()
        {
            var res = Decode(null);
            if (res == null)
                System.Diagnostics.Debug.WriteLine("Decode failed at " + LineParser.LineLeft);
            return res;
        }

        public Object Decode(Object curobj)
        {
            while (true)
            {
                if (LineParser.IsEOL)
                    return curobj;

                Object o = DecodeValue();       // grab new value
                if (o == null)
                    return null;

                bool commamoveon = false;

                if (curobj is JArray)           // if in a jarray
                {
                    if (o is char && (char)o == ']')    // if end marker, jump back
                        return curobj;
                    else
                    {
                        (curobj as JArray).Elements.Add(o);
                        commamoveon = true;
                    }
                }
                else if (curobj is JObject)     // if in a jobject
                {
                    if (o is char && (char)o == '}')    // if end marker, jump back
                        return curobj;
                    else if (o is string && LineParser.IsCharMoveOn(':'))  // ensure we have a string :
                    {
                        string name = o as string;

                        o = DecodeValue();      // get value, into o , so below works
                        if (o == null)
                            return null;

                        (curobj as JObject).Objects[name] = o;  // assign to dictionary

                        commamoveon = true;
                    }
                    else
                        return null;
                }

                if (o is JArray)                // object is a JArray, so we need to jump in and decode it
                {
                    Object finish = Decode(o);  // decode, should return a JArray
                    if (finish == null)
                        return null;
                    if (curobj == null)         // if we are at top, we have our top level value, so return it
                        return finish;
                }
                else if (o is JObject)
                {
                    Object finish = Decode(o);  // decode, should return a JObject
                    if (finish == null)
                        return null;
                    if (curobj == null)         // if we are at top, we have our top level value, so return it
                        return finish;
                }
                else
                {
                    if (curobj == null)        // if we are at top, we have our top level value, so return it
                        return o;               // definition says a JSON can just be a value, so return it
                }

                if (commamoveon)              // if jarray or jobject, end of this value, comma will delimit them
                    LineParser.IsCharMoveOn(',');        // if comma, skip it. if Not, next will be a }
            }
        }

        // return JObject, JArray, char indicating end array/object, string, number, true, false, JNull
        // null if unhappy

        private Object DecodeValue()
        {
            //System.Diagnostics.Debug.WriteLine("Decode at " + p.LineLeft);
            char next = LineParser.PeekChar();

            if (next == '{')
            {
                LineParser.GetChar(true);
                return new JObject();
            }
            else if (next == '}')
            {
                return LineParser.GetChar(true);
            }
            else if (next == '[')
            {
                LineParser.GetChar(true);
                return new JArray();
            }
            else if (next == ']')
            {
                return LineParser.GetChar(true);
            }
            else if (next == '"')  // string
            {
                return LineParser.NextQuotedWord().ReplaceEscapeControlCharsFull();
            }
            else if (char.IsDigit(next) || next == '-')  // number.. json spec says must start with a digit as its integer fraction exponent
            {
                Object o = LineParser.NextLongOrDouble();
                if (o != null)
                    return o;
                else
                    System.Diagnostics.Debug.WriteLine("Failed number " + LineParser.LineLeft);
            }
            else
            {
                if (LineParser.IsStringMoveOn("true"))
                    return true;
                else if (LineParser.IsStringMoveOn("false"))
                    return false;
                else if (LineParser.IsStringMoveOn("null"))
                    return new JNull();
            }

            System.Diagnostics.Debug.WriteLine("JSON Value error " + LineParser.LineLeft);
            return null;
        }

        public static void Verify(Object o)     // verify O is a valid JSON type to be in the tree
        {
            System.Diagnostics.Debug.Assert(o is string || o is double || o is long || o is Boolean || o is IJType);
        }

        public static string ToString(Object o, bool verbose = false, string pad = "  ")
        {
            return verbose ? ToString(o, "", "\r\n", pad) : ToString(o, "", "", "");
        }

        public static string ToString(Object o, string prepad, string postpad, string pad)
        {
            if (o is string)
                return prepad + "\"" + ((string)o).EscapeControlCharsFull() + "\"" + postpad;
            else if (o is double)
                return prepad + ((double)o).ToStringInvariant() + postpad;
            else if (o is long)
                return prepad + ((long)o).ToStringInvariant() + postpad;
            else if (o is Boolean)
                return prepad + ((bool)o).ToString().ToLower() + postpad;
            else if (o is JNull)
                return prepad + "null" + postpad;
            else if (o is JArray)
            {
                string s = prepad + "[" + postpad;
                string prepad1 = prepad + pad;
                JArray ja = o as JArray;
                for (int i = 0; i < ja.Elements.Count; i++)
                {
                    bool notlast = i < ja.Elements.Count - 1;
                    s += ToString(ja.Elements[i], prepad1, postpad, pad);
                    if (notlast)
                    {
                        s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                    }
                }
                s += prepad + "]" + postpad;
                return s;
            }
            else if (o is JObject)
            {
                string s = prepad + "{" + postpad;
                string prepad1 = prepad + pad;
                int i = 0;
                JObject jo = ((JObject)o);
                foreach (var e in jo.Objects)
                {
                    bool notlast = i++ < jo.Objects.Count - 1;
                    if (e.Value is JObject || e.Value is JArray)
                    {
                        s += prepad1 + "\"" + e.Key + "\":" + postpad;
                        s += ToString(e.Value, prepad1, postpad, pad);
                        if (notlast)
                        {
                            s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                        }
                    }
                    else
                    {
                        s += prepad1 + "\"" + e.Key + "\":" + ToString(e.Value, "", "", pad) + (notlast ? "," : "") + postpad;
                    }
                }
                s += prepad + "}" + postpad;
                return s;
            }
            else
                return null;
        }

    }
}

