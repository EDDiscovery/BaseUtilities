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

    // JToken is the base class, can parse, encode

    public abstract class JToken : IEnumerable<JToken>, IEnumerable
    {
        public enum TType { Null, Boolean ,String, Double, Long, Ulong, BigInt, EndObject, EndArray, Object, Array }
        public TType ttype;

        public static implicit operator JToken(string v)
        {
            return new JString(v);
        }
        public static implicit operator JToken(long v)
        {
            return new JLong(v);
        }
        public static implicit operator JToken(ulong v)
        {
            return new JULong(v);
        }
        public static implicit operator JToken(double v)
        {
            return new JDouble(v);
        }
        public static implicit operator JToken(bool v)
        {
            return new JBoolean(v);
        }

        public virtual JToken this[object key] { get { return null; } set { throw new NotImplementedException(); } }

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

        bool IsString { get { return ttype == TType.String; } }
        bool IsInt { get { return ttype == TType.Long || ttype == TType.Ulong || ttype == TType.BigInt; } }
        bool IsDouble { get { return ttype == TType.Double || ttype == TType.Long; } }
        bool IsBool { get { return ttype == TType.Boolean; } }
        bool IsArray { get { return ttype == TType.Array; } }
        bool IsObject { get { return ttype == TType.Object; } }
        bool IsNull { get { return ttype == TType.Null; } }

        public string Str(string def = "")
        {
            return ttype == TType.String ? ((JString)this).StrValue : def;
        }

        public int Int(int def = 0)
        {
            return ttype == TType.Long ? (int)((JLong)this).Value : def;
        }

        public long Long(long def = 0)
        {
            return ttype == TType.Long ? ((JLong)this).Value : def;
        }

        public ulong ULong(ulong def = 0)
        {
            if (ttype == TType.Ulong)
                return ((JULong)this).Value;
            else if (ttype == TType.Long && ((JLong)this).Value >= 0)
                return (ulong)((JLong)this).Value;
            else
                return def;
        }

        public System.Numerics.BigInteger BigInteger(System.Numerics.BigInteger def)
        {
            if (ttype == TType.Ulong)
                return ((JULong)this).Value;
            else if (ttype == TType.Long )
                return (ulong)((JLong)this).Value;
            else if ( ttype == TType.BigInt )
                return ((JBigInteger)this).Value;
            else
                return def;
        }

        public bool Bool(bool def = false)
        {
            return ttype == TType.Boolean ? ((JBoolean)this).Value : def;
        }

        public double Double(double def = 0)
        {
            return ttype == TType.Double ? ((JDouble)this).Value : (ttype == TType.Long ? (double)((JLong)this).Value : def);
        }

        public DateTime? DateTime(System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (ttype == TType.String && System.DateTime.TryParse(((JString)this).StrValue, ci, ds, out DateTime ret))
                return ret;
            else
                return null;
        }

        public DateTime DateTime(DateTime defvalue, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (ttype == TType.String && System.DateTime.TryParse(((JString)this).StrValue, ci, ds, out DateTime ret))
                return ret;
            else
                return defvalue;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool verbose = false, string pad = "  ")
        {
            return verbose ? ToString(this, "", "\r\n", pad) : ToString(this, "", "", "");
        }

        public static string ToString(Object o, string prepad, string postpad, string pad)
        {
            if (o is JString)
                return prepad + "\"" + ((JString)o).StrValue.EscapeControlCharsFull() + "\"" + postpad;
            else if (o is JDouble)
                return prepad + ((JDouble)o).Value.ToStringInvariant() + postpad;
            else if (o is JLong)
                return prepad + ((JLong)o).Value.ToStringInvariant() + postpad;
            else if (o is JULong)
                return prepad + ((JULong)o).Value.ToStringInvariant() + postpad;
            else if (o is JBigInteger)
                return prepad + ((JBigInteger)o).Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + postpad;
            else if (o is JBoolean)
                return prepad + ((JBoolean)o).Value.ToString().ToLower() + postpad;
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
                        s += prepad1 + "\"" + e.Key.EscapeControlCharsFull() + "\":" + postpad;
                        s += ToString(e.Value, prepad1, postpad, pad);
                        if (notlast)
                        {
                            s = s.Substring(0, s.Length - postpad.Length) + "," + postpad;
                        }
                    }
                    else
                    {
                        s += prepad1 + "\"" + e.Key.EscapeControlCharsFull() + "\":" + ToString(e.Value, "", "", pad) + (notlast ? "," : "") + postpad;
                    }
                }
                s += prepad + "}" + postpad;
                return s;
            }
            else
                return null;
        }

        public static JToken Parse(string s)        // null if failed.
        {
            StringParser2 parser = new StringParser2(s);
            return Decode(null, parser, out string unused);
        }

        public static JToken Parse(string s, bool checkeol)        // null if failed - must not be extra text
        {
            StringParser2 parser = new StringParser2(s);
            JToken res = Decode(null, parser, out string unused);
            return parser.IsEOL ? res : null;
        }

        public static JToken Parse(string s, out string error, bool checkeol = false)
        {
            StringParser2 parser = new StringParser2(s);
            JToken res = Decode(null, parser, out error);
            return parser.IsEOL || !checkeol ? res : null;
        }

        // null if its unhappy and error is set
        // decoder does not worry about extra text after the object.

        static private JToken Decode(JToken curobj, StringParser2 parser, out string error)
        {
            error = null;

            while (true)
            {
                if (parser.IsEOL)
                    return curobj;

                int decodestartpos = parser.Position;

                JToken o = DecodeValue(parser);       // grab new value
                if (o == null)
                {
                    error = GenError(parser, decodestartpos);
                    return null;
                }

                bool commamoveon = false;

                if (curobj != null)
                {
                    if (curobj.ttype == TType.Array)           // if in a jarray
                    {
                        if (o.ttype == TType.EndArray)    // if end marker, jump back
                            return curobj;
                        else
                        {
                            (curobj as JArray).Elements.Add(o);
                            commamoveon = true;
                        }
                    }
                    else if (curobj.ttype == TType.Object)     // if in a jobject
                    {
                        if (o.ttype == TType.EndObject)    // if end marker, jump back
                            return curobj;
                        else if (o.ttype == TType.String && parser.IsCharMoveOn(':'))  // ensure we have a string :
                        {
                            string name = ((JString)o).StrValue;
                            decodestartpos = parser.Position;
                            o = DecodeValue(parser);      // get value, into o , so below works
                            if (o == null)
                            {
                                error = GenError(parser, decodestartpos);
                                return null;
                            }

                            (curobj as JObject).Objects[name] = o;  // assign to dictionary

                            commamoveon = true;
                        }
                        else
                            return null;
                    }
                }

                if (o.ttype == TType.Array)                // object is a JArray, so we need to jump in and decode it
                {
                    JToken finish = Decode(o, parser, out error);  // decode, should return a JArray

                    if (finish == null)         // error
                        return null;

                    if (curobj == null)         // if we are at top, we have our top level value, so return it
                        return finish;
                }
                else if (o.ttype == TType.Object)
                {
                    JToken finish = Decode(o, parser, out error);  // decode, should return a JObject

                    if (finish == null)         // error
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
                    parser.IsCharMoveOn(',');        // if comma, skip it. if Not, next will be a }
            }
        }

        // return JObject, JArray, char indicating end array/object, string, number, true, false, JNull
        // null if unhappy

        static JEndObject jendobject = new JEndObject();
        static JEndArray jendarray = new JEndArray();

        static private JToken DecodeValue(StringParser2 parser)
        {
            //System.Diagnostics.Debug.WriteLine("Decode at " + p.LineLeft);
            char next = parser.PeekChar();

            if (next == '{')
            {
                parser.SkipCharAndSkipSpace();
                return new JObject();
            }
            else if (next == '}')
            {
                parser.SkipCharAndSkipSpace();
                return jendobject;
            }
            else if (next == '[')
            {
                parser.SkipCharAndSkipSpace();
                return new JArray();
            }
            else if (next == ']')
            {
                parser.SkipCharAndSkipSpace();
                return jendarray;
            }
            else if (next == '"')  // string
            {
                string value = parser.NextQuotedWord(true).ToString();
                if (value != null)
                    return new JString(value);
            }
            else if (char.IsDigit(next) || next == '-')  // number.. json spec says must start with a digit as its integer fraction exponent
            {
                var ty = parser.NextLongULongBigIntegerOrDouble(out string part, out ulong ulv, out int sign);

                if (ty == StringParser2.ObjectType.Ulong)
                {
                    if (sign == -1)
                        return new JLong(-(long)ulv);
                    else if ( ulv <= long.MaxValue )
                        return new JLong((long)ulv);
                    else
                        return new JULong(ulv);
                }
                else if (ty == StringParser2.ObjectType.Double)
                {
                    if (double.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dv))
                        return new JDouble(dv);
                }
                else if ( ty == StringParser2.ObjectType.BigInt )
                {
                    if (System.Numerics.BigInteger.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out System.Numerics.BigInteger bv))
                        return new JBigInteger(bv);
                }
            }
            else
            {
                if (next == 't' && parser.IsStringMoveOn("true"))
                    return new JBoolean(true);
                else if (next == 'f' && parser.IsStringMoveOn("false"))
                    return new JBoolean(false);
                else if (next == 'n' && parser.IsStringMoveOn("null"))
                    return new JNull();
            }

            return null;
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
    }

    public class JNull : JToken
    {
        public JNull() { ttype = TType.Null; }
    }
    public class JBoolean : JToken
    {
        public JBoolean(bool v) { ttype = TType.Boolean; Value = v; }
        public bool Value { get; set; }
    }
    public class JString : JToken
    {
        public JString(string s) { ttype = TType.String; StrValue = s; }
        public string StrValue;
    }
    public class JLong : JToken
    {
        public JLong(long v) { ttype = TType.Long; Value = v; }
        public long Value { get; set; }
    }
    public class JULong : JToken
    {
        public JULong(ulong v) { ttype = TType.Ulong; Value = v; }
        public ulong Value { get; set; }
    }
    public class JBigInteger : JToken
    {
        public JBigInteger(System.Numerics.BigInteger v) { ttype = TType.BigInt; Value = v; }
        public System.Numerics.BigInteger Value { get; set; }
    }
    public class JDouble : JToken
    {
        public JDouble(double d) { ttype = TType.Double; Value = d; }
        public double Value { get; set; }
    }
    public class JEndObject : JToken    // internal, only used during decode
    {
        public JEndObject() { ttype = TType.EndObject; }
    }
    public class JEndArray : JToken     // internal, only used during decode
    {
        public JEndArray() { ttype = TType.EndArray; }
    }

    public class JObject : JToken, IEnumerable<KeyValuePair<string, JToken>>
    {
        public JObject()
        {
            ttype = TType.Object;
            Objects = new Dictionary<string, JToken>(16);   // giving a small initial cap seems to help
        }

        public Dictionary<string, JToken> Objects { get; set; }

        public override JToken this[object key] { get { System.Diagnostics.Debug.Assert(key is string); return Objects[(string)key]; } set { System.Diagnostics.Debug.Assert(key is string); Objects[(string)key] = value; } }
        public JToken this[string key] { get { return Objects[key]; } set { Objects[key] = value; } }
        public bool ContainsKey(string n) { return Objects.ContainsKey(n); }
        public int Count() { return Objects.Count; }
        public bool Remove(string key) { return Objects.Remove(key); }
        public void Clear() { Objects.Clear(); }

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

        public new IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()  {  return Objects.GetEnumerator(); }
        public override IEnumerator<JToken> GetSubClassTokenEnumerator() { return Objects.Values.GetEnumerator(); }
        public override IEnumerator GetSubClassEnumerator() { return Objects.GetEnumerator(); }
    }

    public class JArray : JToken
    {
        public JArray()
        {
            ttype = TType.Array;
            Elements = new List<JToken>(16);
        }

        public List<JToken> Elements { get; set; }

        public override JToken this[object key] { get { System.Diagnostics.Debug.Assert(key is int); return Elements[(int)key]; } set { System.Diagnostics.Debug.Assert(key is int); Elements[(int)key] = value; } }
        public int Count() { return Elements.Count; }
        public void Add(JToken o) { Elements.Add(o); }
        public void AddRange(IEnumerable<JToken> o) { Elements.AddRange(o); }
        public void RemoveAt(int index) { Elements.RemoveAt(index); }
        public void Clear() { Elements.Clear(); }
        public JToken Find(System.Predicate<JToken> predicate) { return Elements.Find(predicate); }       // find an entry matching the predicate
        public T Find<T>(System.Predicate<JToken> predicate) { Object r = Elements.Find(predicate); return (T)r; }       // find an entry matching the predicate

        public List<string> String() { return Elements.ConvertAll<string>((o) => { return o is JString ? ((JString)o).StrValue : null; }); }
        public List<int> Int() { return Elements.ConvertAll<int>((o) => { return (int)((JLong)o).Value; }); }
        public List<long> Long() { return Elements.ConvertAll<long>((o) => { return ((JLong)o).Value; }); }
        public List<double> Double() { return Elements.ConvertAll<double>((o) => { return ((JDouble)o).Value; }); }

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



