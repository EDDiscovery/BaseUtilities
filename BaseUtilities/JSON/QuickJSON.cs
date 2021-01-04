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
    // small light JSON decoder and encoder.

    // JToken is the base class, can parse, encode.  JArray/JObject for json structures

    public class JToken : IEnumerable<JToken>, IEnumerable
    {
        public enum TType { Null, Boolean, String, Double, Long, ULong, BigInt, Object, Array, EndObject, EndArray, NotPresent }
        public TType TokenType { get; set; }                    // type of token
        public Object Value { get; set; }                       // value of token, if it has one

        public bool IsNotPresent { get { return TokenType == TType.NotPresent; } }
        public bool IsString { get { return TokenType == TType.String; } }
        public bool IsInt { get { return TokenType == TType.Long || TokenType == TType.ULong || TokenType == TType.BigInt; } }
        public bool IsBigInt { get { return TokenType == TType.BigInt; } }
        public bool IsULong { get { return TokenType == TType.ULong; } }
        public bool IsDouble { get { return TokenType == TType.Double || TokenType == TType.Long; } }
        public bool IsBool { get { return TokenType == TType.Boolean; } }
        public bool IsArray { get { return TokenType == TType.Array; } }
        public bool IsObject { get { return TokenType == TType.Object; } }
        public bool IsNull { get { return TokenType == TType.Null; } }

        #region Construction

        public JToken()
        {
            TokenType = TType.NotPresent;
        }

        public JToken(JToken other)
        {
            TokenType = other.TokenType;
            Value = other.Value;
        }

        public JToken(TType t, Object v = null)
        {
            TokenType = t; Value = v;
        }

        static public JToken JNotPresent() { return new JToken(TType.NotPresent); }

        public static implicit operator JToken(string v)        // autoconvert types to JToken types
        {
            if (v == null)
                return new JToken(TType.Null);
            else
                return new JToken(TType.String, v);
        }
        public static implicit operator JToken(long v)
        {
            return new JToken(TType.Long, v);
        }
        public static implicit operator JToken(ulong v)
        {
            return new JToken(TType.ULong, v);
        }
        public static implicit operator JToken(double v)
        {
            return new JToken(TType.Double, v);
        }
        public static implicit operator JToken(bool v)
        {
            return new JToken(TType.Boolean, v);
        }
        public static implicit operator JToken(DateTime v)
        {
            return new JToken(TType.String, v.ToStringZulu());
        }

        public JToken Clone()   // make a copy of the token
        {
            switch (TokenType)
            {
                case TType.Array:
                    {
                        JArray copy = new JArray();
                        foreach (JToken t in this)
                        {
                            copy.Add(t.Clone());
                        }

                        return copy;
                    }
                case TType.Object:
                    {
                        JObject copy = new JObject();
                        foreach (var kvp in (JObject)this)
                        {
                            copy[kvp.Key] = kvp.Value.Clone();
                        }
                        return copy;
                    }
                default:
                    return new JToken(this);
            }
        }

        #endregion

        #region Operators and functions

        public bool DeepEquals(JToken other)
        {
            switch (TokenType)
            {
                case TType.Array:
                    {
                        JArray us = (JArray)this;
                        if (other.TokenType == TType.Array)
                        {
                            JArray ot = (JArray)other;
                            if (ot.Count == us.Count)
                            {
                                for (int i = 0; i < us.Count; i++)
                                {
                                    if (!us[i].DeepEquals(other[i]))
                                        return false;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                case TType.Object:
                    {
                        JObject us = (JObject)this;
                        if (other.TokenType == TType.Object)
                        {
                            JObject ot = (JObject)other;
                            if (ot.Count == us.Count)
                            {
                                foreach (var kvp in us)
                                {
                                    if (!ot.ContainsKey(kvp.Key) || !kvp.Value.DeepEquals(ot[kvp.Key]))       // order unimportant to kvp
                                        return false;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                default:
                    return other.TokenType == this.TokenType && this.Value == other.Value;
            }
        }

        // if called on a non indexed object, return JNotPresent().  
        // On an Array/Object, will return JNotPresent if not present, or indexer is not right type
        public virtual JToken this[object key] { get { return JNotPresent(); } set { throw new NotImplementedException(); } }

        public virtual JToken Contains(string[] ids) { throw new NotImplementedException(); } // lookup one of these keys in a JObject

        public IEnumerator<JToken> GetEnumerator()
        {
            return GetSubClassTokenEnumerator();
        }

        public virtual IEnumerator<JToken> GetSubClassTokenEnumerator() { throw new NotImplementedException(); }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetSubClassEnumerator();
        }

        public virtual IEnumerator GetSubClassEnumerator() { throw new NotImplementedException(); }

        public virtual int Count { get { return 0; } }        // number of children
        public virtual void Clear() { throw new NotImplementedException(); }    // clear all children

        #endregion

        #region Data Get

        public virtual string MultiStr(string[] ids, string def = "")       // multiple lookup in Object of names
        {
            JToken t = Contains(ids);
            return t != null && t.TokenType == TType.String ? (string)Value : def;
        }

        public string Str(string def = "")
        {
            return TokenType == TType.String ? (string)Value : def;
        }

        public string StrNull()
        {
            return TokenType == TType.String ? (string)Value : null;
        }

        public static explicit operator string(JToken t)        // Direct conversion - use Str() safer. Will assert if not string
        {
            if (t.TokenType == TType.Null)
                return null;
            else
                return (string)t.Value;
        }

        public int Int(int def = 0)
        {
            return TokenType == TType.Long ? (int)(long)Value : def;
        }

        public int? IntNull()
        {
            return TokenType == TType.Long ? (int)(long)Value : default(int?);
        }

        public uint UInt(uint def = 0)
        {
            return TokenType == TType.Long && (long)Value >= 0 ? (uint)(long)Value : def;
        }

        public uint? UIntNull()
        {
            return TokenType == TType.Long && (long)Value >= 0 ? (uint)(long)Value : default(uint?);
        }

        public long Long(long def = 0)
        {
            return TokenType == TType.Long ? (long)Value : def;
        }

        public long? LongNull()
        {
            return TokenType == TType.Long ? (long)Value : default(long?);
        }

        public ulong ULong(ulong def = 0)
        {
            if (TokenType == TType.ULong)
                return (ulong)Value;
            else if (TokenType == TType.Long && (long)Value >= 0)
                return (ulong)(long)Value;
            else
                return def;
        }

        public System.Numerics.BigInteger BigInteger(System.Numerics.BigInteger def)
        {
            if (TokenType == TType.ULong)
                return (ulong)Value;
            else if (TokenType == TType.Long && (long)Value >= 0)
                return (ulong)(long)Value;
            else if (TokenType == TType.BigInt)
                return (System.Numerics.BigInteger)Value;
            else
                return def;
        }

        public bool Bool(bool def = false)
        {
            return TokenType == TType.Boolean ? (bool)Value : def;
        }

        public bool? BoolNull()
        {
            return TokenType == TType.Boolean ? (bool)Value : default(bool?);
        }

        public double Double(double def = 0)
        {
            return TokenType == TType.Double ? (double)Value : (TokenType == TType.Long ? (double)(long)Value : def);
        }

        public double? DoubleNull()
        {
            return TokenType == TType.Double ? (double)Value : (TokenType == TType.Long ? (double)(long)Value : default(double?));
        }

        public DateTime? DateTime(System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (TokenType == TType.String && System.DateTime.TryParse((string)Value, ci, ds, out DateTime ret))
                return ret;
            else
                return null;
        }

        public DateTime DateTime(DateTime defvalue, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (TokenType == TType.String && System.DateTime.TryParse((string)Value, ci, ds, out DateTime ret))
                return ret;
            else
                return defvalue;
        }

        public DateTime DateTimeUTC()
        {
            if (TokenType == TType.String && System.DateTime.TryParse((string)Value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime ret))
                return ret;
            else
                return new DateTime(2000, 1, 1);
        }

        public JArray Array()       // null if not
        {
            return this as JArray;
        }

        public JObject Object()     // null if not
        {
            return this as JObject;
        }

        #endregion

        #region ToString rep

        public override string ToString()   // back to JSON form
        {
            return ToString(this, "", "", "", false);
        }

        public string ToStringLiteral()     // data as is, without quoting/escaping strings. Used for data extraction
        {
            return ToString(false, "", "", "", true);
        }

        public string ToString(bool verbose = false, string pad = "  ")
        {
            return verbose ? ToString(this, "", "\r\n", pad, false) : ToString(this, "", "", "", false);
        }

        public static string ToString(JToken o, string prepad, string postpad, string pad, bool stringliterals)
        {
            if (o.TokenType == TType.String)
            {
                if (stringliterals)       // used if your extracting the value of the data as a string, and not turning it back to json.
                    return prepad + (string)o.Value + postpad;
                else
                    return prepad + "\"" + ((string)o.Value).EscapeControlCharsFull() + "\"" + postpad;
            }
            else if (o.TokenType == TType.Double)
                return prepad + ((double)o.Value).ToStringInvariant() + postpad;
            else if (o.TokenType == TType.Long)
                return prepad + ((long)o.Value).ToStringInvariant() + postpad;
            else if (o.TokenType == TType.ULong)
                return prepad + ((ulong)o.Value).ToStringInvariant() + postpad;
            else if (o.TokenType == TType.BigInt)
                return prepad + ((System.Numerics.BigInteger)o.Value).ToString(System.Globalization.CultureInfo.InvariantCulture) + postpad;
            else if (o.TokenType == TType.Boolean)
                return prepad + ((bool)o.Value).ToString().ToLower() + postpad;
            else if (o.TokenType == TType.Null)
                return prepad + "null" + postpad;
            else if (o.TokenType == TType.Array)
            {
                string s = prepad + "[" + postpad;
                string prepad1 = prepad + pad;
                JArray ja = o as JArray;
                for (int i = 0; i < ja.Count; i++)
                {
                    bool notlast = i < ja.Count - 1;
                    s += ToString(ja[i], prepad1, postpad, pad, stringliterals);
                    if (notlast)
                    {
                        s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                    }
                }
                s += prepad + "]" + postpad;
                return s;
            }
            else if (o.TokenType == TType.Object)
            {
                string s = prepad + "{" + postpad;
                string prepad1 = prepad + pad;
                int i = 0;
                JObject jo = ((JObject)o);
                foreach (var e in jo)
                {
                    bool notlast = i++ < jo.Count - 1;
                    if (e.Value is JObject || e.Value is JArray)
                    {
                        s += prepad1 + "\"" + e.Key.EscapeControlCharsFull() + "\":" + postpad;
                        s += ToString(e.Value, prepad1, postpad, pad, stringliterals);
                        if (notlast)
                        {
                            s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                        }
                    }
                    else
                    {
                        s += prepad1 + "\"" + e.Key.EscapeControlCharsFull() + "\":" + ToString(e.Value, "", "", pad, stringliterals) + (notlast ? "," : "") + postpad;
                    }
                }
                s += prepad + "}" + postpad;
                return s;
            }
            else
                return null;
        }

        #endregion

        #region From String

        // null if its unhappy and error is set
        // decoder does not worry about extra text after the object.

        public static JToken Parse(string s)        // null if failed.
        {
            StringParser2 parser = new StringParser2(s);
            return Decode(parser, out string unused);
        }

        public static JToken Parse(string s, bool checkeol)        // null if failed - must not be extra text
        {
            StringParser2 parser = new StringParser2(s);
            JToken res = Decode(parser, out string unused);
            return parser.IsEOL ? res : null;
        }

        public static JToken Parse(string s, out string error, bool checkeol = false)
        {
            StringParser2 parser = new StringParser2(s);
            JToken res = Decode(parser, out error);
            return parser.IsEOL || !checkeol ? res : null;
        }

        static private JToken Decode(StringParser2 parser, out string error)
        {
            error = null;

            JToken[] stack = new JToken[256];
            int sptr = 0;
            bool comma = false;
            JArray curarray = null;
            JObject curobject = null;

            // first decode the first value/object/array
            {
                int decodestartpos = parser.Position;

                JToken o = DecodeValue(parser, false);       // grab new value, not array end

                if (o == null)
                {
                    error = GenError(parser, decodestartpos);
                    return null;
                }
                else if (o.TokenType == TType.Array)
                {
                    stack[++sptr] = o;                      // push this one onto stack
                    curarray = o as JArray;                 // this is now the current array
                }
                else if (o.TokenType == TType.Object)
                {
                    stack[++sptr] = o;                      // push this one onto stack
                    curobject = o as JObject;               // this is now the current object
                }
                else
                {
                    return o;                               // value only
                }
            }

            while (true)
            {
                if (curobject != null)      // if object..
                {
                    while (true)
                    {
                        int decodestartpos = parser.Position;

                        char next = parser.GetChar();

                        if (next == '}')    // end object
                        {
                            parser.SkipSpace();

                            if (comma == true)
                            {
                                error = GenError(parser, decodestartpos);
                                return null;
                            }
                            else
                            {
                                JToken prevtoken = stack[--sptr];
                                if (prevtoken == null)      // if popped stack is null, we are back to beginning, return this
                                {
                                    return stack[sptr + 1];
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                    curobject = prevtoken as JObject;
                                    if (curobject == null)
                                    {
                                        curarray = prevtoken as JArray;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (next == '"')   // property name
                        {
                            string name = parser.NextQuotedWordString(next, true);

                            if (name == null || (comma == false && curobject.Count > 0) || !parser.IsCharMoveOn(':'))
                            {
                                error = GenError(parser, decodestartpos);
                                return null;
                            }
                            else
                            {
                                decodestartpos = parser.Position;

                                JToken o = DecodeValue(parser, false);      // get value

                                if (o == null)
                                {
                                    error = GenError(parser, decodestartpos);
                                    return null;
                                }

                                curobject[name] = o;  // assign to dictionary

                                if (o.TokenType == TType.Array) // if array, we need to change to this as controlling object on top of stack
                                {
                                    if (sptr == stack.Length - 1)
                                    {
                                        error = "Recursion too deep";
                                        return null;
                                    }

                                    stack[++sptr] = o;          // push this one onto stack
                                    curarray = o as JArray;                 // this is now the current object
                                    curobject = null;
                                    comma = false;
                                    break;
                                }
                                else if (o.TokenType == TType.Object)   // if object, this is the controlling object
                                {
                                    if (sptr == stack.Length - 1)
                                    {
                                        error = "Recursion too deep";
                                        return null;
                                    }

                                    stack[++sptr] = o;          // push this one onto stack
                                    curobject = o as JObject;                 // this is now the current object
                                    comma = false;
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                }
                            }
                        }
                        else
                        {
                            error = GenError(parser, decodestartpos);
                            return null;
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        int decodestartpos = parser.Position;

                        JToken o = DecodeValue(parser, true);       // grab new value

                        if (o == null)
                        {
                            error = GenError(parser, decodestartpos);
                            return null;
                        }
                        else if (o.TokenType == TType.EndArray)          // if end marker, jump back
                        {
                            if (comma == true)
                            {
                                error = GenError(parser, decodestartpos);
                                return null;
                            }
                            else
                            {
                                JToken prevtoken = stack[--sptr];
                                if (prevtoken == null)      // if popped stack is null, we are back to beginning, return this
                                {
                                    return stack[sptr + 1];
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                    curobject = prevtoken as JObject;
                                    if (curobject == null)
                                    {
                                        curarray = prevtoken as JArray;
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                        else if ((comma == false && curarray.Count > 0))   // missing comma
                        {
                            error = GenError(parser, decodestartpos);
                            return null;
                        }
                        else
                        {
                            curarray.Add(o);

                            if (o.TokenType == TType.Array) // if array, we need to change to this as controlling object on top of stack
                            {
                                if (sptr == stack.Length - 1)
                                {
                                    error = "Recursion too deep";
                                    return null;
                                }

                                stack[++sptr] = o;              // push this one onto stack
                                curarray = o as JArray;         // this is now the current array
                                comma = false;
                            }
                            else if (o.TokenType == TType.Object) // if object, this is the controlling object
                            {
                                if (sptr == stack.Length - 1)
                                {
                                    error = "Recursion too deep";
                                    return null;
                                }

                                stack[++sptr] = o;              // push this one onto stack
                                curobject = o as JObject;       // this is now the current object
                                curarray = null;
                                comma = false;
                                break;
                            }
                            else
                            {
                                comma = parser.IsCharMoveOn(',');
                            }
                        }
                    }
                }

            }
        }

        static JToken jendarray = new JToken(TType.EndArray);

        // return JObject, JArray, jendarray indicating end array if inarray is set, string, long, ulong, bigint, true, false, JNull
        // null if unhappy

        static private JToken DecodeValue(StringParser2 parser, bool inarray)
        {
            //System.Diagnostics.Debug.WriteLine("Decode at " + p.LineLeft);
            char next = parser.GetChar();
            switch (next)
            {
                case '{':
                    parser.SkipSpace();
                    return new JObject();

                case '[':
                    parser.SkipSpace();
                    return new JArray();

                case '"':
                    string value = parser.NextQuotedWordString(next, true);
                    return value != null ? new JToken(TType.String, value) : null;

                case ']':
                    if (inarray)
                    {
                        parser.SkipSpace();
                        return jendarray;
                    }
                    else
                        return null;

                case '0':       // all positive. JSON does not allow a + at the start (integer fraction exponent)
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    parser.BackUp();
                    return parser.NextJValue(false);
                case '-':
                    return parser.NextJValue(true);
                case 't':
                    return parser.IsStringMoveOn("rue") ? new JToken(TType.Boolean, true) : null;
                case 'f':
                    return parser.IsStringMoveOn("alse") ? new JToken(TType.Boolean, false) : null;
                case 'n':
                    return parser.IsStringMoveOn("ull") ? new JToken(TType.Null) : null;

                default:
                    return null;
            }
        }

        static private string GenError(StringParser2 parser, int start)
        {
            int enderrorpos = parser.Position;
            string s = "JSON Error at " + start + " " + parser.Line.Substring(0, start) + " <ERROR>"
                            + parser.Line.Substring(start, enderrorpos - start) + "</ERROR>" +
                            parser.Line.Substring(enderrorpos);
            System.Diagnostics.Debug.WriteLine(s);
            return s;
        }

        #endregion
    }


    // Object class, holds key/value pairs

    public class JObject : JToken, IEnumerable<KeyValuePair<string, JToken>>
    {
        public JObject()
        {
            TokenType = TType.Object;
            Objects = new Dictionary<string, JToken>(16);   // giving a small initial cap seems to help
        }

        private Dictionary<string, JToken> Objects { get; set; }

        // Returns value or JNotPresent if not present or not string indexor.  jo["fred"].Str() works if fred is not present.  
        public override JToken this[object key]
        {
            get { if (key is string && Objects.TryGetValue((string)key, out JToken v)) return v; else return JNotPresent(); }
            set { System.Diagnostics.Debug.Assert(key is string && value != null); Objects[(string)key] = value; }
        }

        // Returns value or JNotPresent if not present
        public JToken this[string key]
        {
            get { if (Objects.TryGetValue(key, out JToken v)) return v; else return JNotPresent(); }
            set { System.Diagnostics.Debug.Assert(value != null); Objects[key] = value; }
        }

        public bool ContainsKey(string n) { return Objects.ContainsKey(n); }
        public bool TryGetValue(string n, out JToken value) { return Objects.TryGetValue(n, out value); }

        public override JToken Contains(string[] ids)     // see if Object contains one of these keys
        {
            foreach (string key in ids)
            {
                if (Objects.ContainsKey(key))
                    return Objects[key];
            }
            return null;
        }

        public override int Count { get { return Objects.Count; } }

        public void Add(string key, JToken value) { this[key] = value; }
        public bool Remove(string key) { return Objects.Remove(key); }
        public override void Clear() { Objects.Clear(); }

        public new static JObject Parse(string s)        // null if failed.
        {
            var res = JToken.Parse(s);
            return res as JObject;
        }

        public new static JObject Parse(string s, out string error, bool checkeol = false)
        {
            var res = JToken.Parse(s, out error, checkeol);
            return res as JObject;
        }

        public new IEnumerator<KeyValuePair<string, JToken>> GetEnumerator() { return Objects.GetEnumerator(); }
        public override IEnumerator<JToken> GetSubClassTokenEnumerator() { return Objects.Values.GetEnumerator(); }
        public override IEnumerator GetSubClassEnumerator() { return Objects.GetEnumerator(); }
    }

    // Array type

    public class JArray : JToken
    {
        public JArray()
        {
            TokenType = TType.Array;
            Elements = new List<JToken>(16);
        }

        private List<JToken> Elements { get; set; }

        // if out of range, or indexer not int, JNotPresent
        public override JToken this[object key]
        {
            get { if (key is int && (int)key >= 0 && (int)key < Elements.Count) return Elements[(int)key]; else return JNotPresent(); }
            set { System.Diagnostics.Debug.Assert(key is int && value != null); Elements[(int)key] = value; }
        }

        // must be in range.
        public JToken this[int element] { get { return Elements[element]; } set { Elements[element] = value; } }

        // try and get a value.
        public bool TryGetValue(int n, out JToken value) { if (n >= 0 && n < Elements.Count) { value = Elements[n]; return true; } else { value = null; return false; } }

        public override int Count { get { return Elements.Count; } }

        public void Add(JToken o) { Elements.Add(o); }
        public void AddRange(IEnumerable<JToken> o) { Elements.AddRange(o); }
        public void RemoveAt(int index) { Elements.RemoveAt(index); }
        public override void Clear() { Elements.Clear(); }

        public JToken Find(System.Predicate<JToken> predicate) { return Elements.Find(predicate); }       // find an entry matching the predicate
        public T Find<T>(System.Predicate<JToken> predicate) { Object r = Elements.Find(predicate); return (T)r; }       // find an entry matching the predicate

        public List<string> String() { return Elements.ConvertAll<string>((o) => { return o.TokenType == TType.String ? ((string)o.Value) : null; }); }
        public List<int> Int() { return Elements.ConvertAll<int>((o) => { return (int)((long)o.Value); }); }
        public List<long> Long() { return Elements.ConvertAll<long>((o) => { return ((long)o.Value); }); }
        public List<double> Double() { return Elements.ConvertAll<double>((o) => { return ((double)o.Value); }); }

        public override IEnumerator<JToken> GetSubClassTokenEnumerator() { return Elements.GetEnumerator(); }
        public override IEnumerator GetSubClassEnumerator() { return Elements.GetEnumerator(); }

        public new static JArray Parse(string s)        // null if failed.
        {
            var res = JToken.Parse(s);
            return res as JArray;
        }

        public new static JArray Parse(string s, out string error, bool checkeol = false)
        {
            var res = JToken.Parse(s, out error, checkeol);
            return res as JArray;
        }
    }

}



